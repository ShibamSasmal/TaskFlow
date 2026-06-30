import { Component } from '@angular/core';
import { JsonExplorerService, DiffLine } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-compare',
  standalone: false,
  template: `
    <div class="compare-container">
      <div class="inputs-grid" *ngIf="!diffResult">
        <div class="input-pane">
          <span class="pane-title">Original JSON (Source)</span>
          <textarea 
            [(ngModel)]="sourceJson" 
            placeholder="Paste source JSON here..." 
            class="compare-textarea"
            spellcheck="false"
          ></textarea>
        </div>
        <div class="input-pane">
          <span class="pane-title">Modified JSON (Target)</span>
          <textarea 
            [(ngModel)]="targetJson" 
            placeholder="Paste target JSON here..." 
            class="compare-textarea"
            spellcheck="false"
          ></textarea>
        </div>
      </div>

      <div class="actions-bar">
        <button 
          *ngIf="!diffResult" 
          (click)="onCompare()" 
          class="btn-compare-action" 
          [disabled]="loading || !sourceJson || !targetJson"
        >
          {{ loading ? 'Comparing...' : 'Compare JSONs' }}
        </button>
        <button 
          *ngIf="diffResult" 
          (click)="resetCompare()" 
          class="btn-compare-action btn-reset"
        >
          Reset / Edit Inputs
        </button>
      </div>

      <div class="diff-viewer-card" *ngIf="diffResult">
        <div class="card-header">
          <span class="header-text">Comparison Differences</span>
          <div class="diff-legend">
            <span class="legend-item added"><span class="legend-dot"></span> Added</span>
            <span class="legend-item deleted"><span class="legend-dot"></span> Deleted</span>
          </div>
        </div>
        <div class="diff-body">
          <div 
            *ngFor="let line of diffResult" 
            class="diff-line" 
            [class.line-added]="line.type === 'added'"
            [class.line-deleted]="line.type === 'deleted'"
          >
            <div class="line-nums">
              <span class="num-col">{{ line.sourceLineNumber || '' }}</span>
              <span class="num-col">{{ line.targetLineNumber || '' }}</span>
            </div>
            <span class="line-marker">
              {{ line.type === 'added' ? '+' : (line.type === 'deleted' ? '-' : ' ') }}
            </span>
            <pre class="line-code"><code>{{ line.content }}</code></pre>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .compare-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      height: 100%;
    }
    .inputs-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
      height: 400px;
    }
    @media (max-width: 768px) {
      .inputs-grid {
        grid-template-columns: 1fr;
        height: auto;
      }
    }
    .input-pane {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1rem;
    }
    .pane-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: #94a3b8;
    }
    .compare-textarea {
      flex: 1;
      min-height: 300px;
      background: rgba(15, 23, 42, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.05);
      border-radius: 8px;
      padding: 0.75rem;
      color: #38bdf8;
      font-family: 'Fira Code', monospace;
      font-size: 0.8rem;
      line-height: 1.5;
      resize: none;
      outline: none;
    }
    .compare-textarea:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .actions-bar {
      display: flex;
      justify-content: center;
    }
    .btn-compare-action {
      background: #6366f1;
      border: none;
      color: white;
      padding: 0.6rem 1.5rem;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
      font-size: 0.9rem;
      box-shadow: 0 4px 10px rgba(99, 102, 241, 0.25);
      transition: all 0.2s;
    }
    .btn-compare-action:hover:not(:disabled) {
      background: #4f46e5;
      transform: translateY(-1px);
    }
    .btn-compare-action:disabled {
      background: rgba(99, 102, 241, 0.4);
      cursor: not-allowed;
      box-shadow: none;
    }
    .btn-reset {
      background: rgba(255, 255, 255, 0.1);
      color: #cbd5e1;
      border: 1px solid rgba(255, 255, 255, 0.1);
      box-shadow: none;
    }
    .btn-reset:hover {
      background: rgba(255, 255, 255, 0.15);
      color: white;
    }
    .diff-viewer-card {
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1rem;
      display: flex;
      flex-direction: column;
    }
    .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      padding-bottom: 0.5rem;
    }
    .header-text {
      font-weight: 600;
      color: #f8fafc;
      font-size: 0.9rem;
    }
    .diff-legend {
      display: flex;
      gap: 1rem;
      font-size: 0.75rem;
    }
    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.3rem;
      color: #cbd5e1;
    }
    .legend-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    .legend-item.added .legend-dot { background: #22c55e; }
    .legend-item.deleted .legend-dot { background: #ef4444; }

    .diff-body {
      max-height: 500px;
      overflow-y: auto;
      border: 1px solid rgba(255, 255, 255, 0.05);
      background: rgba(15, 23, 42, 0.25);
      border-radius: 8px;
      font-family: 'Fira Code', monospace;
      font-size: 0.8rem;
    }
    .diff-line {
      display: flex;
      align-items: flex-start;
      line-height: 1.5;
      padding: 0.1rem 0;
      border-bottom: 1px solid rgba(255, 255, 255, 0.02);
    }
    .line-added {
      background: rgba(34, 197, 94, 0.08);
      color: #86efac;
    }
    .line-deleted {
      background: rgba(239, 68, 68, 0.08);
      color: #fca5a5;
    }
    .line-nums {
      display: flex;
      width: 60px;
      flex-shrink: 0;
      color: #64748b;
      border-right: 1px solid rgba(255, 255, 255, 0.05);
      padding-right: 4px;
      margin-right: 6px;
      user-select: none;
      font-size: 0.7rem;
    }
    .num-col {
      width: 30px;
      text-align: right;
    }
    .line-marker {
      width: 15px;
      text-align: center;
      flex-shrink: 0;
      color: #64748b;
      font-weight: bold;
      user-select: none;
    }
    .line-code {
      margin: 0;
      white-space: pre-wrap;
      word-break: break-all;
      flex: 1;
      padding-left: 4px;
    }
    .line-code code {
      font-family: inherit;
    }
  `]
})
export class JsonCompareComponent {
  sourceJson: string = '';
  targetJson: string = '';
  diffResult: DiffLine[] | null = null;
  loading: boolean = false;

  constructor(private jsonService: JsonExplorerService) {}

  onCompare(): void {
    if (!this.sourceJson || !this.targetJson) return;
    this.loading = true;
    this.jsonService.compare(this.sourceJson, this.targetJson).subscribe({
      next: (res) => {
        this.diffResult = res;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to compare JSON documents.');
      }
    });
  }

  resetCompare(): void {
    this.diffResult = null;
  }
}
