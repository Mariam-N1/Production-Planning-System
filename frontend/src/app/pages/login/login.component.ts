import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

type Mode = 'login' | 'signup' | 'forgot';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {

  mode: Mode = 'login';

  fullName = '';
  email = '';
  password = '';
  confirm = '';

  showPassword = false;
  showConfirm = false;
  busy = false;
  error = '';
  notice = '';

  // forgot password runs in two steps: ask for a code, then use it
  step: 1 | 2 = 1;
  code = '';

  levels = [1, 2, 3, 4];   // the four strength bars

  // the paint chips that float on the left panel
  chips = ['#6166CF', '#9A8CFF', '#F2B705', '#E0533D', '#2FA37C', '#23264A'];

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.mode = (this.route.snapshot.data['mode'] as Mode) || 'login';
  }

  // ---------- wording ----------
  get title(): string {
    if (this.mode === 'signup') return 'Create your account';
    if (this.mode === 'forgot') return this.step === 1 ? 'Reset your password' : 'Check your email';
    return 'Welcome back';
  }

  get subtitle(): string {
    if (this.mode === 'signup') return 'A few details and your workspace is ready.';
    if (this.mode === 'forgot')
      return this.step === 1
        ? 'Enter your email and we will send you a 6-digit code.'
        : 'We sent a code to ' + this.email + '. It expires in 15 minutes.';
    return 'Sign in to your production planning workspace.';
  }

  get actionLabel(): string {
    if (this.mode === 'signup') return 'Create account';
    if (this.mode === 'forgot') return this.step === 1 ? 'Send code' : 'Save new password';
    return 'Sign in';
  }

  // ---------- which fields this screen shows ----------
  get onReset(): boolean         { return this.mode === 'forgot' && this.step === 2; }
  get showEmail(): boolean       { return this.mode !== 'forgot' || this.step === 1; }
  get showPasswordBox(): boolean { return this.mode !== 'forgot' || this.step === 2; }
  get showConfirmBox(): boolean  { return this.mode === 'signup' || this.onReset; }
  get showStrength(): boolean    { return this.showConfirmBox && !!this.password; }

  get passwordLabel(): string { return this.onReset ? 'New password' : 'Password'; }

  // back to step 1 to ask for another code
  startOver() {
    this.step = 1;
    this.code = '';
    this.password = '';
    this.confirm = '';
    this.error = '';
    this.notice = '';
  }

  go(mode: Mode) {
    this.router.navigate(['/' + mode]);
  }

  // ---------- password strength, on signup and reset ----------
  get strength(): number {
    const p = this.password;
    let score = 0;
    if (p.length >= 8) score++;
    if (/[A-Z]/.test(p)) score++;
    if (/[0-9]/.test(p)) score++;
    if (/[^A-Za-z0-9]/.test(p)) score++;
    return score;
  }

  get strengthLabel(): string {
    return ['', 'Weak', 'Fair', 'Good', 'Strong'][this.strength];
  }

  // live tick / cross under the confirm box
  get confirmState(): '' | 'match' | 'differ' {
    if (!this.confirm) return '';
    return this.password === this.confirm ? 'match' : 'differ';
  }

  // ---------- submit ----------
  submit() {
    if (this.busy) return;
    this.error = '';
    this.notice = '';

    const email = this.email.trim();

    if (this.mode === 'forgot') return this.forgotFlow(email);

    if (!email) return this.fail('Email is required.');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return this.fail('That email does not look right.');
    if (!this.password) return this.fail('Password is required.');

    if (this.mode === 'signup') {
      if (this.fullName.trim().length < 2) return this.fail('Please enter your full name.');
      if (this.password.length < 8) return this.fail('Password must be at least 8 characters.');
      if (this.password !== this.confirm) return this.fail('The two passwords do not match.');
    }

    this.busy = true;
    this.cdr.detectChanges();

    const request = this.mode === 'signup'
      ? this.auth.signup(this.fullName.trim(), email, this.password)
      : this.auth.login(email, this.password);

    request
      .then(() => this.router.navigate(['/home']))
      .catch(err => this.fail(err?.message || 'Could not reach the server. Is the API running?'));
  }

  private forgotFlow(email: string) {
    // step 1 - ask for a code
    if (this.step === 1) {
      if (!email) return this.fail('Email is required.');
      if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) return this.fail('That email does not look right.');

      this.busy = true;
      this.cdr.detectChanges();

      this.auth.forgot(email)
        .then(() => {
          this.busy = false;
          this.step = 2;
          this.cdr.detectChanges();
        })
        .catch(err => this.fail(err?.message || 'Could not reach the server. Is the API running?'));
      return;
    }

    // step 2 - use the code
    if (!/^\d{6}$/.test(this.code.trim())) return this.fail('The code is 6 digits.');
    if (this.password.length < 8) return this.fail('The new password must be at least 8 characters.');
    if (this.password !== this.confirm) return this.fail('The two passwords do not match.');

    this.busy = true;
    this.cdr.detectChanges();

    this.auth.reset(email, this.code.trim(), this.password)
      .then(() => this.router.navigate(['/home']))
      .catch(err => this.fail(err?.message || 'Could not reach the server. Is the API running?'));
  }

  private fail(message: string) {
    this.error = message;
    this.busy = false;
    this.cdr.detectChanges();
  }
}
