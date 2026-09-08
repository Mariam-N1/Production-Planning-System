import { Component } from '@angular/core';
import { PACKING_SCREEN, ScreenConfig } from '../../shared/screen-config';

// Only a config. All behaviour lives in DataScreenComponent.
@Component({
  selector: 'app-packing',
  standalone: false,
  template: '<app-data-screen [config]="screen" />',
})
export class PackingComponent {
  screen: ScreenConfig = PACKING_SCREEN;
}
