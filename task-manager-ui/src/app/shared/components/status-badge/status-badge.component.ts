import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  standalone: false,
  template: `
    <span class="badge status-badge" [ngClass]="normalizedStatus">
      {{ label }}
    </span>
  `,
  styles: [`
    .status-badge {
      display: inline-block;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
      letter-spacing: 0.05em;
    }
    .todo {
      background-color: rgba(148, 163, 184, 0.15);
      color: #94a3b8;
      border: 1px solid rgba(148, 163, 184, 0.3);
    }
    .inprogress {
      background-color: rgba(245, 158, 11, 0.15);
      color: #f59e0b;
      border: 1px solid rgba(245, 158, 11, 0.3);
    }
    .done {
      background-color: rgba(16, 185, 129, 0.15);
      color: #10b981;
      border: 1px solid rgba(16, 185, 129, 0.3);
    }
  `]
})
export class StatusBadgeComponent {
  @Input() status!: string;

  get normalizedStatus(): string {
    if (!this.status) return 'todo';
    const s = this.status.toLowerCase();
    if (s === 'inprogress' || s === 'in progress') return 'inprogress';
    return s;
  }

  get label(): string {
    if (!this.status) return 'Todo';
    if (this.status === 'InProgress') return 'In Progress';
    return this.status;
  }
}
