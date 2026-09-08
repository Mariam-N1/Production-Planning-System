import { Component } from '@angular/core';
import { PRODUCTS_SCREEN, ScreenConfig } from '../../shared/screen-config';

// Only a config. All behaviour lives in DataScreenComponent.
@Component({
  selector: 'app-products',
  standalone: false,
  template: '<app-data-screen [config]="screen" />',
})
export class ProductsComponent {
  screen: ScreenConfig = PRODUCTS_SCREEN;
}
