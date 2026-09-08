import { Injectable } from '@angular/core';

export type ThemeChoice = 'light' | 'dark' | 'system';

// Writes data-theme on <html>. Every dark rule in the CSS hangs off that.
@Injectable({ providedIn: 'root' })
export class ThemeService {

  private key = 'pp-theme';
  choice: ThemeChoice = 'system';

  private media = window.matchMedia('(prefers-color-scheme: dark)');

  constructor() {
    try {
      const saved = localStorage.getItem(this.key) as ThemeChoice | null;
      if (saved === 'light' || saved === 'dark' || saved === 'system') this.choice = saved;
    } catch { }

    // follow the operating system while the choice is "system"
    this.media.addEventListener('change', () => {
      if (this.choice === 'system') this.apply();
    });

    this.apply();
  }

  set(choice: ThemeChoice) {
    this.choice = choice;
    try { localStorage.setItem(this.key, choice); } catch { }
    this.apply();
  }

  // what is actually on screen right now
  get resolved(): 'light' | 'dark' {
    if (this.choice === 'system') return this.media.matches ? 'dark' : 'light';
    return this.choice;
  }

  private apply() {
    document.documentElement.setAttribute('data-theme', this.resolved);
  }
}
