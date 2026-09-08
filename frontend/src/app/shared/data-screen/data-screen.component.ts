import {
  Component, Input, OnInit,
  ChangeDetectorRef, ChangeDetectionStrategy,
} from '@angular/core';
import { MenuItem } from 'primeng/api';
import { Layout } from '../../layout';
import { ColumnDef, ScreenConfig } from '../screen-config'
import { AuthService } from '../../auth/auth.service'

export enum SortOrder {
  Asc  = 'asc',
  Desc = 'desc',
}

//  THE ONE TABLE SCREEN
@Component({
  selector: 'app-data-screen',
  standalone: false,
  templateUrl: './data-screen.component.html',
  styleUrl: './data-screen.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataScreenComponent implements OnInit {

  @Input({ required: true }) config!: ScreenConfig;

  constructor(
    private auth: AuthService,
    public layout: Layout,
    private cdr: ChangeDetectorRef,
  ) {}

  // ---------- columns ----------
  columns: ColumnDef[] = []

  // everything that is a real table column (not form-only)
  get tableColumns(): ColumnDef[] {
    return this.columns.filter(c => c.inTable !== false)
  }

  get shownColumns(): ColumnDef[] {
    return this.tableColumns.filter(c => c.visible !== false)
  }

  // what the Create / Edit dialog shows
  get formColumns(): ColumnDef[] {
    return this.columns.filter(c => c.editable)
  }

  get colCount(): number {
    return this.shownColumns.length || 1
  }

  get lastField(): string {
    const shown = this.shownColumns
    return shown.length ? shown[shown.length - 1].field : ''
  }

  // Which key this cell prints - displayField wins over field.
  cellText(row: any, column: ColumnDef): string {
    const value = row[column.displayField ?? column.field]

    if (column.format === 'swatch')  return value || '—'
    if (column.format === 'dash')    return value || '—'
    if (column.format === 'money')   return '$' + Number(value ?? 0).toFixed(2)
    if (column.format === 'percent') return value + '%'

    return value ?? ''
  }

  // Which query param this column's search box sends.
  filterKey(column: ColumnDef): string {
    return column.filterField ?? column.displayField ?? column.field
  }

  trackById(_index: number, row: any) {
    return row.id
  }

  // ---------- dropdown choices ----------
  options: any = {}

  loadOptions() {
    for (const col of this.columns) {
      if (!col.optionsUrl) continue
      const field = col.field
      // paged endpoints answer { items, total }; plain ones answer an array
      const url = col.optionsUrl + (col.optionsUrl.includes('?') ? '&' : '?') + 'pageSize=200'

      fetch(url, { headers: this.auth.headers() })
        .then(res => res.json())
        .then(data => {
          this.options[field] = Array.isArray(data) ? data : (data.items ?? [])
          this.cdr.detectChanges()
        })
        .catch(err => console.error('Could not load choices for ' + field, err))
    }
  }

  optionsFor(column: ColumnDef): any[] {
    // a fixed list beats a fetched one
    const list = column.optionList ?? this.options[column.field]
    return Array.isArray(list) ? list : []
  }

  optionValueOf(column: ColumnDef, option: any) {
    if (column.optionList) return option.value
    return option[column.optionValue ?? 'id']
  }

  optionLabelOf(column: ColumnDef, option: any) {
    if (column.optionList) return option.label
    return option[column.optionLabel ?? 'name']
  }

  optionColorOf(column: ColumnDef, option: any): string {
    return column.optionColor ? option[column.optionColor] : ''
  }

  // small grey text on a chip, e.g. "5 L"
  optionSubOf(column: ColumnDef, option: any): string {
    if (!column.optionSub) return ''
    const value = option[column.optionSub]
    if (value === null || value === undefined) return ''
    return value + (column.optionSubUnit ? ' ' + column.optionSubUnit : '')
  }

  isTicked(column: ColumnDef, option: any): boolean {
    const chosen = this.newRow[column.field]
    return Array.isArray(chosen) && chosen.includes(this.optionValueOf(column, option))
  }

  toggleTick(column: ColumnDef, option: any) {
    const value = this.optionValueOf(column, option)
    const chosen: any[] = this.newRow[column.field] ?? []

    this.newRow[column.field] = chosen.includes(value)
      ? chosen.filter(v => v !== value)
      : [...chosen, value]

    this.formError = ''
    this.cdr.detectChanges()
  }

  tickedCount(column: ColumnDef): number {
    const chosen = this.newRow[column.field]
    return Array.isArray(chosen) ? chosen.length : 0
  }

  // ---------- remembering the ticks ----------
  get storeKey(): string { return this.config.key + '-columns' }
  get sortKey(): string  { return this.config.key + '-sort' }

  saveColumns() {
    const saved: any = {}
    for (const c of this.tableColumns) saved[c.field] = c.visible
    try { localStorage.setItem(this.storeKey, JSON.stringify(saved)) } catch {}
  }

  loadColumns() {
    let saved: any = null
    try {
      const text = localStorage.getItem(this.storeKey)
      if (text) saved = JSON.parse(text)
    } catch {}
    if (!saved) return

    for (const c of this.columns) {
      if (saved[c.field] !== undefined) c.visible = saved[c.field]
    }
  }

  toggleColumn(column: ColumnDef) {
    if (!column.visible) {
      this.filters[this.filterKey(column)] = ''
      this.applyFilters()
    }
    this.saveColumns()
    this.cdr.detectChanges()
  }

  selectAll() {
    for (const c of this.columns) c.visible = true
    this.saveColumns()
    this.cdr.detectChanges()
  }

  resetColumns() {
    try { localStorage.removeItem(this.storeKey) } catch {}
    for (const c of this.columns) c.visible = true
    this.cdr.detectChanges()
  }

  // ---------- table state ----------
  rows_: any[] = []
  total: number = 0
  filters: any = {}
  topText: string = ''
  first: number = 0
  loading: boolean = false
  loadError: string = ''

  get rows(): number {
    return this.selectedValue || this.total || 1
  }

  ngOnInit() {
    // fresh copy, with the defaults filled in
    this.columns = this.config.columns.map(c => ({
      ...c,
      visible: c.visible !== false,
      inTable: c.inTable !== false,
    }))
    this.sortField = this.config.sortField

    this.loadColumns()
    this.loadSort()
    this.loadOptions()
    this.load()
  }

  // ---------- THE ONE REQUEST ----------
  inFlight: AbortController | null = null

  load() {
    // Cancel whatever is still on its way, so a slow earlier
    if (this.inFlight) this.inFlight.abort()
    const controller = new AbortController()
    this.inFlight = controller

    this.loading = true
    this.loadError = ''
    this.cdr.detectChanges()

    const params = new URLSearchParams()

    if (this.topText) params.set('search', this.topText)

    for (const field in this.filters) {
      if (this.filters[field]) params.set(field, this.filters[field])
    }

    if (this.sortOrder) {
      params.set('sortBy', this.sortField)
      params.set('order', this.sortOrder)
    }

    const size = this.selectedValue
    params.set('page', String(size ? Math.floor(this.first / size) + 1 : 1))
    params.set('pageSize', String(size))

    fetch(this.config.api + '?' + params.toString(), { signal: controller.signal, headers: this.auth.headers() })
      .then(res => {
        if (!this.checkAuth(res)) throw new Error('Session expired')
        if (!res.ok) throw new Error('Server answered ' + res.status)
        return res.json()
      })
      .then(data => {
        this.rows_ = data.items
        this.total = data.total
        this.loading = false
        this.cdr.detectChanges()
      })
      .catch(err => {
        if (err.name === 'AbortError') return
        console.error('Could not load', err)
        this.rows_ = []
        this.total = 0
        this.loading = false
        this.loadError = 'Could not reach the server. Is the API running?'
        this.cdr.detectChanges()
      })
  }

  // 401 = the token is missing or expired
  private checkAuth(res: Response): boolean {
    if (res.status !== 401) return true
    this.auth.logout()
    return false
  }

  getOne(id: any) {
    return fetch(this.config.api + '/' + id, { headers: this.auth.headers() })
      .then(res => this.checkAuth(res) && res.ok ? res.json() : null)   // 404 -> null, not a crash
      .then(row => { this.oneRow = row; return row })
      .catch(err => { console.error('Could not load ' + id, err); return null })
  }

  // ---------- the sort button ----------
  sortField: string = 'title'
  sortOrder: SortOrder | '' = ''

  saveSort() {
    try { localStorage.setItem(this.sortKey, JSON.stringify({ field: this.sortField, order: this.sortOrder })) } catch {}
  }

  loadSort() {
    try {
      const text = localStorage.getItem(this.sortKey)
      if (!text) return
      const saved = JSON.parse(text)
      this.sortField = saved.field ?? this.config.sortField
      this.sortOrder = saved.order ?? ''
    } catch {}
  }

  sortOptions = [
    { label: 'A - Z',        value: SortOrder.Asc  },
    { label: 'Z - A',        value: SortOrder.Desc },
    { label: 'Newest first', value: '' as const },
  ]

  chooseSort(order: SortOrder | '') {
    this.sortOrder = order
    if (order === '') {
      try { localStorage.removeItem(this.sortKey) } catch {}
    } else {
      this.saveSort()
    }
    this.applyFilters()
  }

  // ---------- the Advance Filter button ----------
  showSearch: boolean = true

  toggleSearch() {
    this.showSearch = !this.showSearch
    this.cdr.detectChanges()
  }

  // ---------- the 3-dots MENU on every row ----------
  oneRow: any = null
  menuRow: any = null

  menuItems: MenuItem[] = [
    { label: 'Edit',   icon: 'pi pi-pencil', command: () => this.openEdit(this.menuRow.id) },
    { label: 'Delete', icon: 'pi pi-trash',  command: () => this.askDelete(this.menuRow) },
  ]

  openMenu(row: any, menu: any, event: any) {
    event.stopPropagation()
    this.menuRow = row
    menu.toggle(event)
  }

  // ---------- the "are you sure?" window ----------
  showConfirm: boolean = false
  toDelete: any = null

  askDelete(row: any) {
    this.toDelete = row
    this.showConfirm = true
    this.cdr.detectChanges()
  }

  confirmDelete() {
    this.showConfirm = false
    this.deleteRow(this.toDelete)
  }

  // DELETE answers 204 No Content - empty body, so no res.json()
  deleteRow(row: any) {
    fetch(this.config.api + '/' + row.id, { method: 'DELETE', headers: this.auth.headers() })
      .then(res => {
        if (!this.checkAuth(res)) throw new Error('Session expired')
        if (!res.ok) throw new Error('Delete failed (' + res.status + ')')
        this.load()
      })
      .catch(err => console.error('Could not delete ' + row.id, err))
  }

  // ---------- CREATE / EDIT dialog ----------
  showDialog: boolean = false
  saving: boolean = false
  formError: string = ''
  cancelled: boolean = false
  newRow: any = {}
  editId: any = null

  blankRow(): any {
    const blank: any = {}
    for (const c of this.formColumns) {
      if (c.type === 'multiselect')  blank[c.field] = []
      else if (c.type === 'select')  blank[c.field] = null
      else if (c.type === 'choice')  blank[c.field] = null
      else if (c.type === 'number')  blank[c.field] = null
      else if (c.type === 'color')   blank[c.field] = '#6466f1'   // a sensible starting colour
      else                           blank[c.field] = ''
    }
    return blank
  }

  openDialog() {
    this.loadOptions()          // pick up any shade added since last time
    this.newRow = this.blankRow()
    this.editId = null
    this.formError = ''
    this.cancelled = false
    this.showDialog = true
  }

  openEdit(id: any) {
    this.loadOptions()
    this.newRow = this.blankRow()
    this.editId = id
    this.formError = ''
    this.cancelled = false
    this.showDialog = true

    this.getOne(id).then(row => {
      // the row is gone - someone deleted it, or this list is stale
      if (!row) {
        this.showDialog = false
        this.load()                 // pull a fresh list
        this.cdr.detectChanges()
        return
      }
      const filled: any = {}
      for (const c of this.formColumns) {
        filled[c.field] = c.type === 'multiselect' ? (row[c.field] ?? []) : row[c.field]
      }
      this.newRow = filled
      this.cdr.detectChanges()
    })
  }

  cancelDialog() {
    this.cancelled = true
    this.saving = false
    this.showDialog = false
  }

  saveProduct() {
    // the first text or dropdown field is the "name" of the thing
    const required = this.formColumns.find(c => c.type === 'text' || c.type === 'select')

    if (required) {
      const value = this.newRow[required.field]
      const empty = required.type === 'select' || required.type === 'choice'
        ? (value === null || value === undefined || value === '')
        : !String(value ?? '').trim()
      if (empty) {
        this.formError = required.header + ' is required.'
        return
      }
    }

    this.saving = true
    this.formError = ''
    this.cancelled = false

    const editing = this.editId !== null
    const url = editing ? this.config.api + '/' + this.editId : this.config.api

    const body: any = {}
    for (const c of this.formColumns) {
      const value = this.newRow[c.field]

      if (c.type === 'multiselect')  body[c.field] = (value ?? []).map((v: any) => Number(v))
      else if (c.type === 'select')  body[c.field] = Number(value) || 0
      else if (c.type === 'choice')  body[c.field] = String(value ?? '')
      else if (c.type === 'number')  body[c.field] = Number(value) || 0
      else                           body[c.field] = String(value ?? '').trim()
    }

    fetch(url, {
      method: editing ? 'PUT' : 'POST',
      headers: this.auth.headers(),
      body: JSON.stringify(body),
    })
      .then(async res => {
        if (!this.checkAuth(res)) throw new Error('Session expired')
        if (!res.ok) {
          const problem = await res.json().catch(() => null)
          throw new Error(this.readErrors(problem))
        }
        return res.json()
      })
      .then(() => {
        if (this.cancelled) return
        this.saving = false
        this.showDialog = false
        this.applyFilters()
      })
      .catch(err => {
        console.error('Could not save', err)
        if (this.cancelled) return
        this.formError = err.message || 'Could not save. Please try again.'
        this.saving = false
        this.cdr.detectChanges()
      })
  }

  readErrors(problem: any): string {
    if (problem && problem.errors) {
      const messages: string[] = []
      for (const field in problem.errors) {
        for (const m of problem.errors[field]) messages.push(m)
      }
      if (messages.length) return messages.join(' ')
    }
    return (problem && problem.title) || 'Could not save. Please try again.'
  }

  // ---------- typing ----------
  searchTimer: any = null

  queueLoad() {
    clearTimeout(this.searchTimer)
    this.searchTimer = setTimeout(() => this.applyFilters(), 300)
  }

  onSearch(text: any) {
    this.topText = text.trim()
    this.queueLoad()
  }

  search(text: any, column: ColumnDef) {
    this.filters[this.filterKey(column)] = text.trim()
    this.queueLoad()
  }

  applyFilters() {
    this.first = 0
    this.load()
  }

  onPageChange(event: any) {
    this.first = event.first ?? 0
    this.load()
  }

  dropdownOptions = [
    { label: '10',  value: 10 },
    { label: '20',  value: 20 },
    { label: '30',  value: 30 },
    { label: '50',  value: 50 },
    { label: 'All', value: 0  },
  ]

  selectedValue: number = 10

  changeSize(size: any) {
    this.selectedValue = size
    this.first = 0
    this.load()
  }

  get rangeText(): string {
    if (!this.total) return '0 items'
    return (this.first + 1) + '-' + (this.first + this.rows_.length)
         + ' of ' + this.total + ' items'
  }
}
