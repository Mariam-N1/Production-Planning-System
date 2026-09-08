import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { Layout } from '../../layout';
import { AuthService } from '../../auth/auth.service';
import { API } from '../../shared/api';

type Status = 'Pending' | 'Approved' | 'Completed' | 'Rejected';

@Component({
  selector: 'app-approvals',
  standalone: false,
  templateUrl: './approvals.component.html',
  styleUrl: './approvals.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApprovalsComponent implements OnInit {

  private api = API + '/ProductionRuns';

  tab: Status = 'Pending';
  tabs: Status[] = ['Pending', 'Approved', 'Completed', 'Rejected'];

  runs: any[] = [];
  counts: any = { pending: 0, approved: 0, completed: 0, rejected: 0 };
  pendingLitres = 0;
  oldestPending: string | null = null;

  loading = false;
  loadError = '';
  busyId: number | null = null;

  // ---------- the new-request form ----------
  formOpen = false;
  products: any[] = [];
  shades: any[] = [];
  packings: any[] = [];
  form = { productId: 0, shadeId: 0, packingId: 0, quantity: 0, note: '' };
  saving = false;
  formError = '';

  // ---------- the reject box ----------
  rejectingId: number | null = null;
  rejectReason = '';

  constructor(
    public layout: Layout,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.load();
    this.loadChoices();
  }

  countFor(status: Status): number {
    return this.counts[status.toLowerCase()] ?? 0;
  }

  show(status: Status) {
    if (this.tab === status) return;
    this.tab = status;
    this.rejectingId = null;
    this.load();
  }

  load() {
    this.loading = true;
    this.loadError = '';
    this.cdr.detectChanges();

    fetch(this.api + '?status=' + this.tab, { headers: this.auth.headers() })
      .then(res => {
        if (res.status === 401) { this.auth.logout(); throw new Error('Session expired'); }
        if (!res.ok) throw new Error('Server answered ' + res.status);
        return res.json();
      })
      .then(data => {
        this.runs = data.items ?? [];
        this.counts = data.counts ?? this.counts;
        this.pendingLitres = data.pendingLitres ?? 0;
        this.oldestPending = data.oldestPending ?? null;
        this.loading = false;
        this.cdr.detectChanges();
      })
      .catch(err => {
        if (err.message === 'Session expired') return;
        console.error('Could not load the runs', err);
        this.runs = [];
        this.loading = false;
        this.loadError = 'Could not reach the server. Is the API running?';
        this.cdr.detectChanges();
      });
  }

  // the three dropdowns on the form
  loadChoices() {
    const grab = (url: string) =>
      fetch(url + (url.includes('?') ? '&' : '?') + 'pageSize=200', { headers: this.auth.headers() })
        .then(res => res.ok ? res.json() : null)
        .then(data => Array.isArray(data) ? data : (data?.items ?? []))
        .catch(() => []);

    Promise.all([
      grab(API + '/Products'),
      grab(API + '/Shades'),
      grab(API + '/Packings'),
    ]).then(([products, shades, packings]) => {
      this.products = products;
      this.shades = shades;
      this.packings = packings;
      this.cdr.detectChanges();
    });
  }

  // ---------- actions ----------
  approve(run: any) { this.act(run.id, 'approve'); }
  complete(run: any) { this.act(run.id, 'complete'); }

  startReject(run: any) {
    this.rejectingId = run.id;
    this.rejectReason = '';
    this.cdr.detectChanges();
  }

  confirmReject(run: any) {
    this.act(run.id, 'reject', { reason: this.rejectReason.trim() });
  }

  private act(id: number, what: string, body?: any) {
    if (this.busyId) return;
    this.busyId = id;
    this.loadError = '';
    this.cdr.detectChanges();

    fetch(this.api + '/' + id + '/' + what, {
      method: 'PUT',
      headers: this.auth.headers(),
      body: JSON.stringify(body ?? {}),
    })
      .then(async res => {
        if (res.status === 401) { this.auth.logout(); throw new Error('Session expired'); }
        if (!res.ok) {
          const problem = await res.json().catch(() => null);
          throw new Error(this.auth.readErrors(problem, res.status));
        }
      })
      .then(() => {
        this.busyId = null;
        this.rejectingId = null;
        this.load();       // reload, so the counts and the list both refresh
      })
      .catch(err => {
        if (err.message === 'Session expired') return;
        this.busyId = null;
        this.loadError = err.message;
        this.cdr.detectChanges();
      });
  }

  // ---------- the form ----------
  openForm() {
    this.form = { productId: 0, shadeId: 0, packingId: 0, quantity: 0, note: '' };
    this.formError = '';
    this.formOpen = true;
    this.cdr.detectChanges();
  }

  closeForm() {
    this.formOpen = false;
    this.cdr.detectChanges();
  }

  // live preview of the litres the batch works out to
  get formLitres(): number {
    const pack = this.packings.find(p => p.id === Number(this.form.packingId));
    if (!pack) return 0;
    return Number(this.form.quantity || 0) * Number(pack.size || 0);
  }

  submit() {
    if (this.saving) return;
    this.formError = '';

    if (!Number(this.form.productId)) return this.failForm('Choose a product.');
    if (!Number(this.form.shadeId))   return this.failForm('Choose a shade.');
    if (!Number(this.form.packingId)) return this.failForm('Choose a pack size.');
    if (Number(this.form.quantity) < 1) return this.failForm('Enter how many units to make.');

    this.saving = true;
    this.cdr.detectChanges();

    fetch(this.api, {
      method: 'POST',
      headers: this.auth.headers(),
      body: JSON.stringify({
        productId: Number(this.form.productId),
        shadeId: Number(this.form.shadeId),
        packingId: Number(this.form.packingId),
        quantity: Number(this.form.quantity),
        note: this.form.note.trim(),
      }),
    })
      .then(async res => {
        if (res.status === 401) { this.auth.logout(); throw new Error('Session expired'); }
        if (!res.ok) {
          const problem = await res.json().catch(() => null);
          throw new Error(this.auth.readErrors(problem, res.status));
        }
      })
      .then(() => {
        this.saving = false;
        this.formOpen = false;
        this.tab = 'Pending';    // the new row lives there
        this.load();
      })
      .catch(err => {
        if (err.message === 'Session expired') return;
        this.failForm(err.message);
      });
  }

  private failForm(message: string) {
    this.formError = message;
    this.saving = false;
    this.cdr.detectChanges();
  }

  // ---------- display helpers ----------
  private dateFmt = new Intl.DateTimeFormat('en-GB', {
    timeZone: 'Asia/Karachi', day: '2-digit', month: 'short', year: 'numeric',
  });

  stamp(raw: string | null): string {
    if (!raw) return '';
    const d = new Date(raw);
    return isNaN(d.getTime()) ? '' : this.dateFmt.format(d);
  }

  // "2 hours ago", "3 days ago"
  ago(raw: string | null): string {
    if (!raw) return '';
    const then = new Date(raw).getTime();
    if (isNaN(then)) return '';

    const mins = Math.floor((Date.now() - then) / 60000);
    if (mins < 1)  return 'just now';
    if (mins < 60) return mins + (mins === 1 ? ' minute ago' : ' minutes ago');

    const hours = Math.floor(mins / 60);
    if (hours < 24) return hours + (hours === 1 ? ' hour ago' : ' hours ago');

    const days = Math.floor(hours / 24);
    return days + (days === 1 ? ' day ago' : ' days ago');
  }

  // a pending request older than two days gets an amber note
  isStale(raw: string | null): boolean {
    if (!raw) return false;
    const then = new Date(raw).getTime();
    return !isNaN(then) && (Date.now() - then) > 2 * 24 * 60 * 60 * 1000;
  }

  get oldestText(): string {
    return this.oldestPending ? this.ago(this.oldestPending).replace(' ago', '') : '';
  }

  // white text on a dark swatch, dark text on a pale one
  isDark(hex: string): boolean {
    if (!hex || hex.length < 7) return false;
    const r = parseInt(hex.slice(1, 3), 16);
    const g = parseInt(hex.slice(3, 5), 16);
    const b = parseInt(hex.slice(5, 7), 16);
    return (0.299 * r + 0.587 * g + 0.114 * b) / 255 < 0.55;
  }
}
