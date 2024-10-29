import { Routes } from '@angular/router';
import { ReportListComponent } from './report-list/report-list.component';
import { MeterDataComponent } from './meter-data/meter-data.component';

export const routes: Routes = [
    { path: 'reports', component: ReportListComponent },
    { path: 'meter-data', component: MeterDataComponent },
    { path: '', redirectTo: '/reports', pathMatch: 'full' }
];

