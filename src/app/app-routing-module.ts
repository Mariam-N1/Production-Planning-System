import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { HomeComponent } from './pages/home/home.component';
import { ProductsComponent } from './pages/products/products.component';
import { PaintComponent } from './pages/paint/paint.component';
import { PackingComponent } from './pages/packing/packing.component';
import { ApprovalsComponent } from './pages/approvals/approvals.component';
import { SettingsComponent } from './pages/settings/settings.component';
import { LoginComponent } from './pages/login/login.component';

import { authGuard, guestGuard } from './auth/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  // one component, three modes - guestGuard keeps signed-in users out
  { path: 'login',  component: LoginComponent, canActivate: [guestGuard], data: { mode: 'login'  } },
  { path: 'signup', component: LoginComponent, canActivate: [guestGuard], data: { mode: 'signup' } },
  { path: 'forgot', component: LoginComponent, canActivate: [guestGuard], data: { mode: 'forgot' } },

  // everything below needs a token
  { path: 'home',      component: HomeComponent,      canActivate: [authGuard] },
  { path: 'products',  component: ProductsComponent,  canActivate: [authGuard] },
  { path: 'paint',     component: PaintComponent,     canActivate: [authGuard] },
  { path: 'packing',   component: PackingComponent,   canActivate: [authGuard] },
  { path: 'approvals', component: ApprovalsComponent, canActivate: [authGuard] },
  { path: 'settings',  component: SettingsComponent,  canActivate: [authGuard] },

  { path: 'planning', redirectTo: 'products' },

  { path: '**', redirectTo: 'home' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
