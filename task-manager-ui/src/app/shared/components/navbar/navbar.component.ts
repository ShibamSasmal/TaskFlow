import { Component } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-navbar',
  standalone: false,
  template: `
    <nav class="navbar">
      <div class="navbar-container">
        <a class="navbar-brand" routerLink="/dashboard">
          <span class="brand-icon">🗂</span>
          <span class="brand-text">TaskFlow</span>
        </a>
        
        <div class="navbar-menu" *ngIf="authService.isAuthenticated()">
          <a class="nav-link" routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
          <a class="nav-link" routerLink="/tasks" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Tasks</a>
          <a class="nav-link" routerLink="/resume-analyzer" routerLinkActive="active">Resume Analyzer</a>
        </div>

        <div class="navbar-actions">
          <ng-container *ngIf="authService.isAuthenticated(); else guestUser">
            <span class="user-greeting">
              Hello, <span class="username">{{ username }}</span>
            </span>
            <button class="btn btn-logout" (click)="onLogout()">Logout</button>
          </ng-container>
          <ng-template #guestUser>
            <a class="nav-link" routerLink="/auth/login" routerLinkActive="active">Login</a>
            <a class="btn btn-primary" routerLink="/auth/register">Register</a>
          </ng-template>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      background: rgba(15, 23, 42, 0.8);
      backdrop-filter: blur(12px);
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
      position: sticky;
      top: 0;
      z-index: 1000;
      padding: 0.75rem 0;
    }
    .navbar-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      text-decoration: none;
      font-size: 1.25rem;
      font-weight: 800;
      color: #f8fafc;
      letter-spacing: -0.025em;
    }
    .brand-icon {
      font-size: 1.5rem;
    }
    .navbar-menu {
      display: flex;
      align-items: center;
      gap: 1.5rem;
      margin-left: 2.5rem;
      margin-right: auto;
    }
    .nav-link {
      color: #94a3b8;
      text-decoration: none;
      font-size: 0.95rem;
      font-weight: 500;
      transition: color 0.2s;
    }
    .nav-link:hover, .nav-link.active {
      color: #6366f1;
    }
    .navbar-actions {
      display: flex;
      align-items: center;
      gap: 1.25rem;
    }
    .user-greeting {
      font-size: 0.9rem;
      color: #94a3b8;
    }
    .username {
      color: #f8fafc;
      font-weight: 600;
    }
    .btn {
      padding: 0.5rem 1rem;
      border-radius: 8px;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      transition: all 0.2s;
      border: none;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
    .btn-logout {
      background: rgba(239, 68, 68, 0.1);
      color: #ef4444;
      border: 1px solid rgba(239, 68, 68, 0.2);
    }
    .btn-logout:hover {
      background: #ef4444;
      color: #ffffff;
      transform: translateY(-1px);
    }
    .btn-primary {
      background: #6366f1;
      color: #ffffff;
      box-shadow: 0 4px 10px rgba(99, 102, 241, 0.3);
    }
    .btn-primary:hover {
      background: #4f46e5;
      transform: translateY(-1px);
    }
  `]
})
export class NavbarComponent {
  constructor(public authService: AuthService, private router: Router) {}

  get username(): string {
    const token = this.authService.getToken();
    if (!token) return '';
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload['unique_name'] || payload['name'] || payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User';
    } catch {
      return 'User';
    }
  }

  onLogout() {
    this.authService.logout();
  }
}
