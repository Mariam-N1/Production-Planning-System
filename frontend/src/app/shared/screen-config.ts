import { API } from './api';

//  ONE SHAPE FOR EVERY TABLE SCREEN

export interface ColumnDef {
  field: string            // the key in the JSON, and the value sent when saving
  header: string           // what the user reads

  // ---- table ----
  inTable?: boolean        // default true. false = form-only (e.g. the shade ticks)
  visible?: boolean        // default true. the column-chooser toggles this
  width?: string
  holder?: string          // placeholder in that column's search box
  displayField?: string    // cell shows THIS instead of `field`
                           // e.g. field 'productId' but display 'productTitle'
  filterField?: string     // query param the search box sends
                           // (searching Product must filter by NAME, not id)
  format?: 'money' | 'percent' | 'dash' | 'swatch'   // swatch = a colour circle

  // ---- form ----
  editable?: boolean       // default false. true = appears in Create/Edit
  type?: 'text' | 'number' | 'select' | 'choice' | 'color' | 'multiselect'
                           // select = a foreign key (a number)
  wide?: boolean           // full width in the dialog

  // ---- for select / multiselect ----
  optionsUrl?: string      // where the choices come from
  optionLabel?: string     // which property of an option to show
  optionValue?: string     // which property to send. default 'id'
  optionList?: { value: string; label: string }[]   // for 'choice'
  optionColor?: string     // option property holding a hex -> shows a circle
  optionSub?: string       // second property shown as small grey text
  optionSubUnit?: string   // appended to optionSub, e.g. 'L'
}

export interface ScreenConfig {
  key: string              // 'products' - names the localStorage keys
  breadcrumb: string
  title: string
  api: string              // the endpoint this screen talks to
  searchHolder: string
  sortField: string        // what the A-Z button sorts by
  columns: ColumnDef[]
}

//  PRODUCTS  - the main table. No foreign keys of its own.
export const PRODUCTS_SCREEN: ScreenConfig = {
  key: 'products',
  breadcrumb: 'Planning | Products',
  title: 'Products',
  api: `${API}/Products`,
  searchHolder: 'Search here by title or category',
  sortField: 'title',
  columns: [
    { field: 'id',       header: 'Serial No', holder: 'Search No',       width: '120px', type: 'number' },
    { field: 'title',    header: 'Title',     holder: 'Search Title',    width: '260px', type: 'text',   editable: true, wide: true },
    { field: 'category', header: 'Category',  holder: 'Search Category', width: '160px', type: 'text',   editable: true },
    { field: 'discount', header: 'Discount',  holder: 'Search Discount', width: '130px', type: 'number', editable: true, format: 'percent' },
    { field: 'price',    header: 'Price',     holder: 'Search Price',    width: '130px', type: 'number', editable: true },
    { field: 'stock',    header: 'Stock',     holder: 'Search Stock',    width: '120px', type: 'number', editable: true },

    // Form-only. The list comes from the Shades screen, so adding a
    // shade there makes it appear here on its own.
    { field: 'shadeIds', header: 'Shades', inTable: false, editable: true, wide: true,
      type: 'multiselect', optionsUrl: `${API}/Shades`,
      optionLabel: 'shadeName', optionColor: 'colorCode' },

    { field: 'packingIds', header: 'Packings', inTable: false, editable: true, wide: true,
      type: 'multiselect', optionsUrl: `${API}/Packings`,
      optionLabel: 'packingName', optionSub: 'size', optionSubUnit: 'L' },
  ],
}

//  PAINT  - the product_shades bridge.
export const PAINT_SCREEN: ScreenConfig = {
  key: 'paint',
  breadcrumb: 'Planning | Shades',
  title: 'Shades',
  api: `${API}/Shades`,
  searchHolder: 'Search here by shade',
  sortField: 'shadeName',
  columns: [
    { field: 'id', header: 'ID', holder: 'Search ID', width: '100px' },

    { field: 'shadeName', header: 'Shade', holder: 'Search Shade', width: '280px',
      type: 'text', editable: true, wide: true },

    // 'color' -> a PrimeNG colour picker. 'swatch' -> a circle in the cell.
    { field: 'colorCode', header: 'Hex Code', holder: 'Search Hex', width: '200px',
      type: 'color', editable: true, format: 'swatch' },
  ],
}


// ============================================================
//  PACKING
// ============================================================
export const PACKING_SCREEN: ScreenConfig = {
  key: 'packing',
  breadcrumb: 'Planning | Packing',
  title: 'Packing',
  api: `${API}/Packings`,
  searchHolder: 'Search here by packing name',
  sortField: 'packingName',
  columns: [
    { field: 'id', header: 'ID', holder: 'Search ID', width: '100px' },

    { field: 'packingName', header: 'Packing Name', holder: 'Search Name', width: '280px',
      type: 'text', editable: true, wide: true },

    { field: 'size', header: 'Packing Size (Litres)', holder: 'Search Size', width: '240px',
      type: 'number', editable: true },
  ],
}
