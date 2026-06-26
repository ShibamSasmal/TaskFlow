import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { SharedModule } from '../shared/shared.module';

import { TaskListComponent } from './task-list/task-list.component';
import { TaskFormComponent } from './task-form/task-form.component';
import { TaskCardComponent } from './task-card/task-card.component';

@NgModule({
  declarations: [
    TaskListComponent,
    TaskFormComponent,
    TaskCardComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    SharedModule
  ],
  exports: [
    TaskListComponent,
    TaskFormComponent,
    TaskCardComponent
  ]
})
export class TasksModule {}
