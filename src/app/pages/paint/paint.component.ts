import { Component } from '@angular/core';
import { PAINT_SCREEN, ScreenConfig } from '../../shared/screen-config';

// Only a config. All behaviour lives in DataScreenComponent.
@Component({
  selector: 'app-paint',
  standalone: false,
  template: '<app-data-screen [config]="screen" />',
})
export class PaintComponent {
  screen: ScreenConfig = PAINT_SCREEN;
}
