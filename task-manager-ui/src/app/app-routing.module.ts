import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { TaskListComponent } from './tasks/task-list/task-list.component';
import { TaskFormComponent } from './tasks/task-form/task-form.component';
import { AuthGuard } from './shared/guards/auth.guard';
import { ResumeAnalyzerComponent } from './resume-analyzer/resume-analyzer.component';
import { AnalysisDetailComponent } from './resume-analyzer/analysis-detail.component';
import { JsonExplorerComponent } from './json-explorer/json-explorer.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent }
    ]
  },
  {
    path: 'dashboard',
    component: DashboardComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'tasks',
    canActivate: [AuthGuard],
    children: [
      { path: '', component: TaskListComponent },
      { path: 'new', component: TaskFormComponent },
      { path: ':id/edit', component: TaskFormComponent }
    ]
  },
  {
    path: 'resume-analyzer',
    canActivate: [AuthGuard],
    children: [
      { path: '', component: ResumeAnalyzerComponent },
      { path: 'analysis/:id', component: AnalysisDetailComponent }
    ]
  },
  {
    path: 'json-explorer',
    component: JsonExplorerComponent,
    canActivate: [AuthGuard]
  },
  { path: '**', redirectTo: '/dashboard' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
