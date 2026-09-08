using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendProject.Data;
using BackendProject.Models;
using BackendProject.Services;
using System.Security.Cryptography;

namespace BackendProject.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly EmailSender _email;

        // Microsoft's hasher. It salts each password and uses PBKDF2,
        // so two people with the same password get different hashes.
        private static readonly PasswordHasher<User> _hasher = new();

        public AuthController(AppDbContext context, IConfiguration config, EmailSender email)
        {
            _context = context;
            _config = config;
            _email = email;
        }

        // POST /api/Auth/signup
        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _context.Users.AnyAsync(u => u.Email == email))
                return BadRequest(Error("Email", "That email is already registered."));

            // the very first account has to be an Admin, or nobody could
            // ever approve anything
            var first = !await _context.Users.AnyAsync();

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = email,
                CreatedAt = DateTime.UtcNow,
                Role = first ? Roles.Admin : Roles.User,
            };

            // hash it - the plain password is never stored
            user.PasswordHash = _hasher.HashPassword(user, dto.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(Session(user));
        }

        // POST /api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Same message whether the email is unknown or the password is
            // wrong - telling them which one leaks who has an account.
            if (user == null)
                return BadRequest(Error("Email", "Email or password is incorrect."));

            // still locked out from earlier failures
            if (user.LockedUntil != null && user.LockedUntil > DateTime.UtcNow)
            {
                var minutes = Math.Max(1, (int)Math.Ceiling((user.LockedUntil.Value - DateTime.UtcNow).TotalMinutes));
                return BadRequest(Error("Email",
                    $"Too many failed attempts. Please try again in {minutes} minute{(minutes == 1 ? "" : "s")}."));
            }

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                user.FailedLogins++;

                // five misses buys a fifteen minute wait
                if (user.FailedLogins >= 5)
                {
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLogins = 0;
                }

                await _context.SaveChangesAsync();
                return BadRequest(Error("Email", "Email or password is incorrect."));
            }

            // a good password wipes the slate
            if (user.FailedLogins != 0 || user.LockedUntil != null)
            {
                user.FailedLogins = 0;
                user.LockedUntil = null;
                await _context.SaveChangesAsync();
            }

            return Ok(Session(user));
        }



        // ---------- forgot password ----------

        // POST /api/Auth/forgot
        [HttpPost("forgot")]
        public async Task<IActionResult> Forgot(ForgotDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user != null)
            {
                // don't let someone spam the inbox - one code a minute
                var justSent = user.ResetExpiresAt != null
                            && user.ResetExpiresAt > DateTime.UtcNow.AddMinutes(14);

                if (!justSent)
                {
                    // a real random 6-digit code, not Random()
                    var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

                    user.ResetCodeHash = _hasher.HashPassword(user, code);
                    user.ResetExpiresAt = DateTime.UtcNow.AddMinutes(15);
                    user.ResetAttempts = 0;
                    await _context.SaveChangesAsync();

                    // a mail failure must not tell the caller the address exists
                    try { await _email.SendCodeAsync(user.Email, user.FullName, code); }
                    catch (Exception ex) { Console.WriteLine("Email failed: " + ex.Message); }
                }
            }

            // Same answer either way. Otherwise this page tells anyone
            // who asks which email addresses have an account.
            return Ok(new { message = "If that email is registered, a code is on its way." });
        }

        // POST /api/Auth/reset
        [HttpPost("reset")]
        public async Task<IActionResult> Reset(ResetDto dto)
        {
            var email = dto.Email.Trim().ToLower();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            var dead = user == null
                    || string.IsNullOrEmpty(user.ResetCodeHash)
                    || user.ResetExpiresAt == null
                    || user.ResetExpiresAt < DateTime.UtcNow;

            if (dead)
                return BadRequest(Error("Code", "That code has expired. Please ask for a new one."));

            // five guesses, then the code is dead - a million combinations
            // means nothing if you can try them all
            if (user!.ResetAttempts >= 5)
            {
                ClearReset(user);
                await _context.SaveChangesAsync();
                return BadRequest(Error("Code", "Too many attempts. Please ask for a new code."));
            }

            var check = _hasher.VerifyHashedPassword(user, user.ResetCodeHash, dto.Code);

            if (check == PasswordVerificationResult.Failed)
            {
                user.ResetAttempts++;
                await _context.SaveChangesAsync();
                return BadRequest(Error("Code", "That code is not correct."));
            }

            user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
            ClearReset(user);                 // one use only

            // a successful reset also clears a lockout
            user.FailedLogins = 0;
            user.LockedUntil = null;

            await _context.SaveChangesAsync();

            return Ok(Session(user));         // signed in straight away
        }

        private static void ClearReset(User user)
        {
            user.ResetCodeHash = string.Empty;
            user.ResetExpiresAt = null;
            user.ResetAttempts = 0;
        }

        // ---------- everything below needs a valid token ----------

        // Finds the signed-in user from the "sub" claim inside the token.
        private async Task<Models.User?> CurrentUser()
        {
            // this.User is the ClaimsPrincipal from the token, not the model
            var raw = this.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                   ?? this.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(raw, out var id)
                ? await _context.Users.FindAsync(id)
                : null;
        }

        // GET /api/Auth/me
        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me()
        {
            var user = await CurrentUser();
            if (user == null) return Unauthorized();

            return Ok(new { user.Id, user.FullName, user.Email, user.CreatedAt, user.Role });
        }

        // PUT /api/Auth/profile
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile(ProfileDto dto)
        {
            var user = await CurrentUser();
            if (user == null) return Unauthorized();

            var email = dto.Email.Trim().ToLower();

            // someone else may already own that address
            if (await _context.Users.AnyAsync(u => u.Email == email && u.Id != user.Id))
                return BadRequest(Error("Email", "That email is already registered."));

            user.FullName = dto.FullName.Trim();
            user.Email = email;
            await _context.SaveChangesAsync();

            // the email is inside the token, so a changed email needs a new one
            return Ok(Session(user));
        }

        // PUT /api/Auth/password
        [Authorize]
        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var user = await CurrentUser();
            if (user == null) return Unauthorized();

            var check = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);

            if (check == PasswordVerificationResult.Failed)
                return BadRequest(Error("CurrentPassword", "Your current password is not correct."));

            if (dto.CurrentPassword == dto.NewPassword)
                return BadRequest(Error("NewPassword", "The new password must be different."));

            user.PasswordHash = _hasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            // a fresh token, so the old one cannot be reused
            return Ok(Session(user));
        }

        // DELETE /api/Auth/account
        [Authorize]
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount(DeleteAccountDto dto)
        {
            var user = await CurrentUser();
            if (user == null) return Unauthorized();

            var check = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (check == PasswordVerificationResult.Failed)
                return BadRequest(Error("Password", "That password is not correct."));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Builds the signed token plus the details the UI shows.
        private object Session(User user)
        {
            var key = _config["Jwt:Key"] ?? "";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            // Claims are facts about the user, carried inside the token.
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new { user.Id, user.FullName, user.Email, user.CreatedAt, user.Role },
            };
        }

        private static object Error(string field, string message)
            => new { errors = new Dictionary<string, string[]> { [field] = new[] { message } } };
    }
}
