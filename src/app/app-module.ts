import { NgModule } from '@angular/core';

import { BrowserModule } from '@angular/platform-browser';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// ---- PrimeNG theme ----
import Aura from '@primeuix/themes/aura';
import { definePreset } from '@primeuix/themes';
import { providePrimeNG } from 'primeng/config';

const MyTheme = definePreset(Aura, {
  semantic: {
    primary: {
      50:  '#F5F3FF',
      100: '#F1EDFF',
      200: '#E4DCFF',
      300: '#CDBEFF',
      400: '#9A8CFF',
      500: '#6166CF',
      600: '#5A5FBE',
      700: '#4E52A6',
      800: '#42468C',
      900: '#373A73',
      950: '#23264A',
    },
  },
});

// ---- PrimeNG components ----
import { TableModule } from 'primeng/table';    // <p-table>
import { SelectModule } from 'primeng/select';  // <p-select>
import { PaginatorModule } from 'primeng/paginator'; // <p-paginator>
import { DialogModule } from 'primeng/dialog';       // <p-dialog>
import { MenuModule } from 'primeng/menu';           // <p-menu>
import { PopoverModule } from 'primeng/popover';     // <p-popover>
import { ColorPickerModule } from 'primeng/colorpicker'; // <p-colorpicker>

// ---- my files ----
import { AppRoutingModule } from './app-routing-module';
import { App } from './app';
import { DataScreenComponent } from './shared/data-screen/data-screen.component';
import { ProductsComponent } from './pages/products/products.component';
import { PaintComponent } from './pages/paint/paint.component';
import { PackingComponent } from './pages/packing/packing.component';
import { HomeComponent } from './pages/home/home.component';
import { ApprovalsComponent } from './pages/approvals/approvals.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';

@NgModule({
  declarations: [
    App,
    DataScreenComponent,   // the shared table screen
    ProductsComponent,     // three thin wrappers around it
    PaintComponent,
    PackingComponent,
    HomeComponent,
    ApprovalsComponent,
    SettingsComponent,
    LoginComponent,
  ],
  imports: [
    BrowserModule,
    CommonModule,
    AppRoutingModule,
    FormsModule,      // needed for [(ngModel)] on the dropdown
    TableModule,
    SelectModule,
    PaginatorModule,
    DialogModule,
    MenuModule,
    PopoverModule,
    ColorPickerModule,
  ],
  providers: [
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: MyTheme,
        options: {
          darkModeSelector: '[data-theme="dark"]',   // PrimeNG follows the same switch
          cssLayer: { name: 'primeng', order: 'theme, base, primeng' },
        },
      },
    }),
  ],
  bootstrap: [App],
})
export class AppModule {}