import { Routes } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { LoginComponent } from './login/login.component';

export const routes: Routes = [
    {path:'',component:LoginComponent,data: { navbarType: 'none' }},
    {path:'userhome',component:DashboardComponent,data: { navbarType: 'customer' }},
    {path:'Settings',component:DashboardComponent,data: { navbarType: 'customer' }},
];
