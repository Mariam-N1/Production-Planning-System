import { ChangeDetectionStrategy, ChangeDetectorRef, Component } from '@angular/core';
import { Layout } from '../../layout';
import { AuthService } from '../../auth/auth.service';
import { ThemeChoice, ThemeService } from '../../shared/theme.service';

@Component({
  selector: 'settings-home',
  standalone: false,
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent {

  // ---------- profile ----------
  editing = false;
  fullName = '';
  email = '';
  profileBusy = false;
  profileError = '';
  profileDone = '';

  // ---------- password ----------
  current = '';
  fresh = '';
  confirm = '';
  showCurrent = false;
  showFresh = false;
  passwordBusy = false;
  passwordError = '';
  passwordDone = '';

  // ---------- delete ----------
  confirmingDelete = false;
  deletePassword = '';
  showDeletePassword = false;
  deleteBusy = false;
  deleteError = '';

  themes: { value: ThemeChoice; label: string; hint: string }[] = [
    { value: 'light',  label: 'Light',  hint: 'Always the light theme' },
    { value: 'dark',   label: 'Dark',   hint: 'Always the dark theme' },
    { value: 'system', label: 'System', hint: 'Follow your Mac' },
  ];

  constructor(
    public layout: Layout,
    public auth: AuthService,
    public theme: ThemeService,
    private cdr: ChangeDetectorRef,
  ) {}

  get memberSince(): string {
    const raw = this.auth.user?.createdAt;
    if (!raw) return '';
    const d = new Date(raw);
    if (isNaN(d.getTime())) return '';
    return new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Asia/Karachi',
      day: '2-digit', month: 'long', year: 'numeric',
    }).format(d);
  }

  // ---------- profile ----------
  startEdit() {
    this.fullName = this.auth.user?.fullName ?? '';
    this.email = this.auth.user?.email ?? '';
    this.profileError = '';
    this.profileDone = '';
    this.editing = true;
  }

  cancelEdit() {
    this.editing = false;
    this.profileError = '';
  }

  saveProfile() {
    if (this.profileBusy) return;
    this.profileError = '';
    this.profileDone = '';

    if (this.fullName.trim().length < 2) return this.stop('profile', 'Please enter your full name.');
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(this.email.trim()))
      return this.stop('profile', 'That email does not look right.');

    this.profileBusy = true;
    this.cdr.detectChanges();

    this.auth.updateProfile(this.fullName.trim(), this.email.trim())
      .then(() => {
        this.profileBusy = false;
        this.editing = false;
        this.profileDone = 'Profile updated.';
        this.cdr.detectChanges();
      })
      .catch(err => this.stop('profile', err.message));
  }

  // ---------- password ----------
  get passwordsMatch(): '' | 'match' | 'differ' {
    if (!this.confirm) return '';
    return this.fresh === this.confirm ? 'match' : 'differ';
  }

  savePassword() {
    if (this.passwordBusy) return;
    this.passwordError = '';
    this.passwordDone = '';

    if (!this.current) return this.stop('password', 'Enter your current password.');
    if (this.fresh.length < 8) return this.stop('password', 'The new password must be at least 8 characters.');
    if (this.fresh !== this.confirm) return this.stop('password', 'The two new passwords do not match.');
    if (this.fresh === this.current) return this.stop('password', 'The new password must be different.');

    this.passwordBusy = true;
    this.cdr.detectChanges();

    this.auth.changePassword(this.current, this.fresh)
      .then(() => {
        this.passwordBusy = false;
        this.current = this.fresh = this.confirm = '';
        this.passwordDone = 'Password changed. Your old one no longer works.';
        this.cdr.detectChanges();
      })
      .catch(err => this.stop('password', err.message));
  }

  // ---------- danger zone ----------
  removeAccount() {
    if (this.deleteBusy) return;
    this.deleteError = '';

    if (!this.deletePassword) return this.stop('delete', 'Enter your password to confirm.');

    this.deleteBusy = true;
    this.cdr.detectChanges();

    this.auth.deleteAccount(this.deletePassword)
      .catch(err => this.stop('delete', err.message));
  }

  signOut() { this.auth.logout(); }

  setTheme(choice: ThemeChoice) {
    this.theme.set(choice);
    this.cdr.detectChanges();
  }

  private stop(which: 'profile' | 'password' | 'delete', message: string) {
    if (which === 'profile')  { this.profileError = message;  this.profileBusy = false; }
    if (which === 'password') { this.passwordError = message; this.passwordBusy = false; }
    if (which === 'delete')   { this.deleteError = message;   this.deleteBusy = false; }
    this.cdr.detectChanges();
  }
}
