import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { JsonExplorerService, Snippet } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-history',
  standalone: false,
  template: `
    <div class="history-container">
      <div class="history-layout">
        <!-- Favorites Panel -->
        <div class="snippet-pane">
          <div class="pane-header">
            <span class="pane-title font-semibold text-slate-200">⭐ Favorite Snippets</span>
          </div>
          
          <div class="snippets-list">
            <div *ngFor="let fav of favorites" class="snippet-card">
              <div class="snippet-info">
                <span class="snippet-lbl">{{ fav.label }}</span>
                <span class="snippet-time">{{ fav.timestamp | date:'short' }}</span>
              </div>
              <div class="snippet-actions">
                <button (click)="onLoad(fav.json)" class="btn-snip-action btn-snip-load">Load</button>
                <button (click)="onDeleteFav(fav.id)" class="btn-snip-action btn-snip-delete">Delete</button>
              </div>
            </div>
            <div *ngIf="favorites.length === 0" class="empty-state">
              No snippets favorited yet. Save snippets in the Editor view.
            </div>
          </div>
        </div>

        <!-- History Panel -->
        <div class="snippet-pane">
          <div class="pane-header">
            <span class="pane-title font-semibold text-slate-200">🕒 Recent History (Last 20)</span>
            <button (click)="onClearHistory()" class="btn-clear-history" *ngIf="history.length > 0">Clear History</button>
          </div>

          <div class="snippets-list">
            <div *ngFor="let hist of history" class="snippet-card">
              <div class="snippet-info">
                <span class="snippet-lbl">{{ hist.label }}</span>
                <span class="snippet-time">{{ hist.timestamp | date:'mediumTime' }}</span>
              </div>
              <div class="snippet-actions">
                <button (click)="onLoad(hist.json)" class="btn-snip-action btn-snip-load">Load</button>
              </div>
            </div>
            <div *ngIf="history.length === 0" class="empty-state">
              History is empty. Format, convert, or validate JSONs to see them here.
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .history-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    .history-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    @media (max-width: 768px) {
      .history-layout {
        grid-template-columns: 1fr;
      }
    }
    .snippet-pane {
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
      min-height: 400px;
    }
    .pane-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      padding-bottom: 0.5rem;
    }
    .pane-title {
      font-size: 0.9rem;
    }
    .btn-clear-history {
      background: transparent;
      border: none;
      color: #ef4444;
      font-size: 0.75rem;
      cursor: pointer;
      font-weight: 600;
    }
    .btn-clear-history:hover {
      text-decoration: underline;
    }
    .snippets-list {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      max-height: 380px;
      overflow-y: auto;
    }
    .snippet-card {
      background: rgba(255, 255, 255, 0.02);
      border: 1px solid rgba(255, 255, 255, 0.04);
      border-radius: 8px;
      padding: 0.75rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      transition: all 0.2s;
    }
    .snippet-card:hover {
      background: rgba(255, 255, 255, 0.04);
      border-color: rgba(255, 255, 255, 0.08);
    }
    .snippet-info {
      display: flex;
      flex-direction: column;
      gap: 0.2rem;
    }
    .snippet-lbl {
      font-size: 0.8rem;
      font-weight: 600;
      color: #cbd5e1;
      word-break: break-all;
    }
    .snippet-time {
      font-size: 0.7rem;
      color: #64748b;
    }
    .snippet-actions {
      display: flex;
      gap: 0.4rem;
    }
    .btn-snip-action {
      border: none;
      padding: 0.25rem 0.6rem;
      border-radius: 4px;
      font-size: 0.75rem;
      font-weight: bold;
      cursor: pointer;
      transition: all 0.2s;
    }
    .btn-snip-load {
      background: rgba(99, 102, 241, 0.1);
      color: #a5b4fc;
      border: 1px solid rgba(99, 102, 241, 0.2);
    }
    .btn-snip-load:hover {
      background: #6366f1;
      color: white;
    }
    .btn-snip-delete {
      background: rgba(239, 68, 68, 0.1);
      color: #fca5a5;
      border: 1px solid rgba(239, 68, 68, 0.2);
    }
    .btn-snip-delete:hover {
      background: #ef4444;
      color: white;
    }
    .empty-state {
      padding: 2rem;
      text-align: center;
      color: #64748b;
      font-size: 0.85rem;
    }
  `]
})
export class JsonHistoryComponent implements OnInit {
  @Output() loadJson = new EventEmitter<string>();

  history: Snippet[] = [];
  favorites: Snippet[] = [];

  constructor(private jsonService: JsonExplorerService) {}

  ngOnInit(): void {
    this.refreshLists();
  }

  refreshLists(): void {
    this.history = this.jsonService.getHistory();
    this.favorites = this.jsonService.getFavorites();
  }

  onLoad(json: string): void {
    this.loadJson.emit(json);
  }

  onDeleteFav(id: string): void {
    this.jsonService.removeFavorite(id);
    this.refreshLists();
  }

  onClearHistory(): void {
    if (confirm('Are you sure you want to clear your JSON viewing history?')) {
      this.jsonService.clearHistory();
      this.refreshLists();
    }
  }
}
