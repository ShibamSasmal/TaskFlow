import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-priority-badge',
  standalone: false,
  template: `
    <span class="badge priority-badge" [ngClass]="priority.toLowerCase() || 'medium'">
      {{ priority || 'Medium' }}
    </span>
  `,
  styles: [`
    .priority-badge {
      display: inline-block;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .low {
      background-color: rgba(59, 130, 246, 0.15);
      color: #3b82f6;
      border: 1px solid rgba(59, 130, 246, 0.3);
    }
    .medium {
      background-color: rgba(245, 158, 11, 0.15);
      color: #f59e0b;
      border: 1px solid rgba(245, 158, 11, 0.3);
    }
    .high {
      background-color: rgba(239, 68, 68, 0.15);
      color: #ef4444;
      border: 1px solid rgba(239, 68, 68, 0.3);
    }
  `]
})
export class PriorityBadgeComponent {
  @Input() priority!: string;
}
