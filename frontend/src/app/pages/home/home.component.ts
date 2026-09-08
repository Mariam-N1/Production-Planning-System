import { Component, OnInit, OnDestroy, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { Layout } from '../../layout';
import { AuthService } from '../../auth/auth.service';
import { API } from '../../shared/api';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit, OnDestroy {

  api = API + '/Dashboard';

  data: any = null;
  loading = true;

  // the live clock, always Pakistan time
  now = '';
  private clock: any = null;

  // Built once, reused every tick - creating a formatter is not cheap.
  // Intl takes real timezone names, so no offset maths and no DST worries.
  private fmt = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Asia/Karachi',
    weekday: 'short',
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    hour12: true,
  });
  loadError = '';
  copied = '';

  constructor(
    public layout: Layout,
    private auth: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.tick();                                        // show it straight away
    this.clock = setInterval(() => this.tick(), 1000);  // then every second
    this.load();
  }

  ngOnDestroy() {
    clearInterval(this.clock);   // stop the timer when you leave the screen
  }

  tick() {
    this.now = this.fmt.format(new Date());
    this.cdr.detectChanges();    // OnPush - nudge Angular to repaint
  }

  load() {
    this.loading = true;
    this.loadError = '';
    this.cdr.detectChanges();

    fetch(this.api, { headers: this.auth.headers() })
      .then(res => {
        if (res.status === 401) { this.auth.logout(); throw new Error('Session expired'); }
        if (!res.ok) throw new Error('Server answered ' + res.status);
        return res.json();
      })
      .then(data => {
        this.data = data;
        this.loading = false;
        this.cdr.detectChanges();
      })
      .catch(err => {
        console.error('Could not load the dashboard', err);
        this.loading = false;
        this.loadError = 'Could not reach the server. Is the API running?';
        this.cdr.detectChanges();
      });
  }

  // ---------- helpers the template uses ----------

  // dark colours need light text on the fan deck
  isDark(hex: string): boolean {
    if (!hex) return false;
    const h = hex.replace('#', '');
    if (h.length < 6) return false;
    const r = parseInt(h.slice(0, 2), 16);
    const g = parseInt(h.slice(2, 4), 16);
    const b = parseInt(h.slice(4, 6), 16);
    // standard luminance - below 0.55 reads as dark
    return (0.299 * r + 0.587 * g + 0.114 * b) / 255 < 0.55;
  }

  // the widest bar sets the scale for the rest
  topShadeWidth(count: number): string {
    const top = this.data?.topShades?.[0]?.count || 1;
    return Math.round((count / top) * 100) + '%';
  }

  stockClass(stock: number): string {
    if (stock === 0) return 'out';
    if (stock < 10) return 'low';
    return 'fine';
  }

  stockText(stock: number): string {
    if (stock === 0) return 'out of stock';
    if (stock < 10) return stock + ' left';
    return stock + ' in stock';
  }

  copyHex(hex: string) {
    try { navigator.clipboard.writeText(hex); } catch { }
    this.copied = hex;
    this.cdr.detectChanges();
    setTimeout(() => { this.copied = ''; this.cdr.detectChanges(); }, 1800);
  }

  go(path: string) {
    this.router.navigate([path]);
  }
}
