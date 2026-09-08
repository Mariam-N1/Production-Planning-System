import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { API } from '../shared/api';

export interface AppUser {
  id: number;
  fullName: string;
  email: string;
  createdAt: string;
  role: string;          // "User" or "Admin"
}

// One place that knows who is signed in.
@Injectable({ providedIn: 'root' })
export class AuthService {

  private api = API + '/Auth';

  private tokenKey = 'pp-token';
  private userKey = 'pp-user';

  token: string | null = null;
  user: AppUser | null = null;

  constructor(private router: Router) {
    this.restore();
  }

  // localStorage survives a refresh, so a reload keeps you signed in
  private restore() {
    try {
      this.token = localStorage.getItem(this.tokenKey);
      const raw = localStorage.getItem(this.userKey);
      this.user = raw ? JSON.parse(raw) : null;
    } catch { }
  }

  private keep(token: string, user: AppUser) {
    this.token = token;
    this.user = user;
    try {
      localStorage.setItem(this.tokenKey, token);
      localStorage.setItem(this.userKey, JSON.stringify(user));
    } catch { }
  }

  // only Admins can approve, reject or complete a run
  get isAdmin(): boolean {
    return this.user?.role === 'Admin';
  }

  get isSignedIn(): boolean {
    if (!this.token) return false;

    // an expired token is the same as no token
    if (this.expiresAt && Date.now() >= this.expiresAt) {
      this.clear();
      return false;
    }
    return true;
  }

  // reads the "exp" claim out of the token, in milliseconds
  private get expiresAt(): number | null {
    try {
      const payload = JSON.parse(atob(this.token!.split('.')[1]));
      return payload.exp ? payload.exp * 1000 : null;
    } catch {
      return null;
    }
  }

  private clear() {
    this.token = null;
    this.user = null;
    try {
      localStorage.removeItem(this.tokenKey);
      localStorage.removeItem(this.userKey);
    } catch { }
  }

  // the two initials shown in the avatar
  get initials(): string {
    const name = this.user?.fullName?.trim() || '';
    if (!name) return '?';
    const parts = name.split(/\s+/);
    const first = parts[0]?.[0] ?? '';
    const last = parts.length > 1 ? parts[parts.length - 1][0] : '';
    return (first + last).toUpperCase();
  }

  // every protected request sends this header
  headers(): Record<string, string> {
    const h: Record<string, string> = { 'Content-Type': 'application/json' };
    if (this.token) h['Authorization'] = 'Bearer ' + this.token;
    return h;
  }

  signup(fullName: string, email: string, password: string) {
    return this.send('/signup', { fullName, email, password });
  }

  login(email: string, password: string) {
    return this.send('/login', { email, password });
  }

  private send(path: string, body: any) {
    return fetch(this.api + path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
      .then(async res => {
        const data = await res.json().catch(() => null);

        if (!res.ok) {
          // 404 = this endpoint is not on the server that is running
          if (res.status === 404)
            throw new Error('The API has no ' + path + ' endpoint. Stop it and run "dotnet run" again so the new code loads.');

          throw new Error(this.readErrors(data, res.status));
        }

        if (!data || !data.token) throw new Error('The server replied without a token.');
        return data;
      })
      .catch(err => {
        // fetch itself failed - the API is not running at all
        if (err instanceof TypeError)
          throw new Error('Could not reach the API at ' + this.api + '. Is it running?');
        throw err;
      })
      .then(data => {
        this.keep(data.token, data.user);
        return data.user as AppUser;
      });
  }

  // ---------- forgot password ----------

  // always answers the same way, whether or not the email is registered
  forgot(email: string) {
    return this.plain('/forgot', { email });
  }

  reset(email: string, code: string, newPassword: string) {
    return this.send('/reset', { email, code, newPassword });
  }

  // like send(), but the reply carries no token
  private plain(path: string, body: any) {
    return fetch(this.api + path, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body),
    })
      .then(async res => {
        const data = await res.json().catch(() => null);

        if (!res.ok) {
          if (res.status === 404)
            throw new Error('The API has no ' + path + ' endpoint. Stop it and run "dotnet run" again so the new code loads.');
          throw new Error(this.readErrors(data, res.status));
        }
        return data;
      })
      .catch(err => {
        if (err instanceof TypeError)
          throw new Error('Could not reach the API at ' + this.api + '. Is it running?');
        throw err;
      });
  }

  // ---------- settings screen ----------

  updateProfile(fullName: string, email: string) {
    return this.sendAuthed('/profile', 'PUT', { fullName, email })
      .then(data => { this.keep(data.token, data.user); return data.user as AppUser });
  }

  changePassword(currentPassword: string, newPassword: string) {
    return this.sendAuthed('/password', 'PUT', { currentPassword, newPassword })
      .then(data => { this.keep(data.token, data.user); return data.user as AppUser });
  }

  deleteAccount(password: string) {
    return this.sendAuthed('/account', 'DELETE', { password }, true)
      .then(() => this.logout());
  }

  // same as send(), but carries the token and can expect an empty reply
  private sendAuthed(path: string, method: string, body: any, noContent = false) {
    return fetch(this.api + path, {
      method,
      headers: this.headers(),
      body: JSON.stringify(body),
    })
      .then(async res => {
        if (res.status === 401) { this.logout(); throw new Error('Your session expired. Please sign in again.') }

        if (noContent && res.ok) return null;

        const data = await res.json().catch(() => null);

        if (!res.ok) {
          if (res.status === 404)
            throw new Error('The API has no ' + path + ' endpoint. Stop it and run "dotnet run" again so the new code loads.');
          throw new Error(this.readErrors(data, res.status));
        }
        return data;
      })
      .catch(err => {
        if (err instanceof TypeError)
          throw new Error('Could not reach the API at ' + this.api + '. Is it running?');
        throw err;
      });
  }

  logout() {
    this.clear();
    this.router.navigate(['/login']);
  }

  // the API's validation reply, as one readable line
  readErrors(problem: any, status?: number): string {
    if (problem && problem.errors) {
      const messages: string[] = [];
      for (const field in problem.errors) {
        const value = problem.errors[field];
        if (Array.isArray(value)) messages.push(...value);
        else if (value) messages.push(String(value));
      }
      if (messages.length) return messages.join(' ');
    }

    if (problem && (problem.detail || problem.title)) return problem.detail || problem.title;

    return status
      ? 'The server answered ' + status + '.'
      : 'Something went wrong. Please try again.';
  }
}
