import { Component, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { Layout } from './layout';
import { AuthService } from './auth/auth.service';
import { ThemeService } from './shared/theme.service';

@Component({
  selector: 'app-root',
  standalone: false,
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  // true = the Products / Paint / Packing flyout is showing
  screensOpen = false;

  // true = the account card above the memoji is showing
  accountOpen = false;

  constructor(
    public layout: Layout,
    public auth: AuthService,
    public theme: ThemeService,   // constructing it applies the saved theme
    private router: Router,
  ) {}

  // login / signup / forgot render on their own, without the sidebar
  get isAuthPage(): boolean {
    const url = this.router.url.split('?')[0];
    return url.startsWith('/login') || url.startsWith('/signup') || url.startsWith('/forgot');
  }

  // "Member since 12 Mar 2026"
  get memberSince(): string {
    const raw = this.auth.user?.createdAt;
    if (!raw) return '';
    const d = new Date(raw);
    if (isNaN(d.getTime())) return '';
    return new Intl.DateTimeFormat('en-GB', {
      timeZone: 'Asia/Karachi',
      day: '2-digit', month: 'short', year: 'numeric',
    }).format(d);
  }

  // a click anywhere else closes the account card
  @HostListener('document:click')
  closeAccount() { this.accountOpen = false; }

  signOut() {
    this.accountOpen = false;
    this.auth.logout();
  }
}
