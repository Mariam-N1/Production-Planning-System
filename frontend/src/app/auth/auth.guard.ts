import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

// Sits in front of a route. No token, no entry.
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isSignedIn) return true;

  router.navigate(['/login']);
  return false;
};

// The reverse: keeps a signed-in user off the login page.
export const guestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isSignedIn) return true;

  router.navigate(['/home']);
  return false;
};
