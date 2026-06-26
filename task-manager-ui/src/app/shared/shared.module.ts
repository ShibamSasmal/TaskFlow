import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { NavbarComponent } from './components/navbar/navbar.component';
import { PriorityBadgeComponent } from './components/priority-badge/priority-badge.component';
import { StatusBadgeComponent } from './components/status-badge/status-badge.component';
import { StatCardComponent } from './components/stat-card/stat-card.component';

@NgModule({
  declarations: [
    NavbarComponent,
    PriorityBadgeComponent,
    StatusBadgeComponent,
    StatCardComponent
  ],
  imports: [
    CommonModule,
    RouterModule
  ],
  exports: [
    NavbarComponent,
    PriorityBadgeComponent,
    StatusBadgeComponent,
    StatCardComponent,
    CommonModule
  ]
})
export class SharedModule {}
