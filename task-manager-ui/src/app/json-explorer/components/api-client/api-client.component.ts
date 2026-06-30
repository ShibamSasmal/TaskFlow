import { Component, EventEmitter, Output } from '@angular/core';
import { JsonExplorerService } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-api-client',
  standalone: false,
  template: `
    <div class="api-client-container">
      <div class="api-card">
        <span class="card-title font-semibold text-slate-200">REST API Response Viewer</span>
        
        <div class="api-input-row">
          <span class="method-badge">GET</span>
          <input 
            type="url" 
            [(ngModel)]="apiUrl" 
            placeholder="Enter HTTP GET API endpoint URL (e.g. https://api.github.com/repos/angular/angular)..." 
            class="input-api-url"
            (keyup.enter)="onFetch()"
          />
          <button 
            (click)="onFetch()" 
            class="btn-fetch" 
            [disabled]="loading || !apiUrl"
          >
            {{ loading ? 'Fetching...' : 'Send Request' }}
          </button>
        </div>

        <div class="api-response-section" *ngIf="responseJson || errorMsg">
          <div class="pane-header">
            <span class="pane-title text-slate-300">HTTP Response payload</span>
            <div class="actions-group" *ngIf="responseJson">
              <button (click)="onLoadInEditor()" class="btn-api-action btn-load">⚡ Load in Editor</button>
              <button (click)="copyResponse()" class="btn-api-action">{{ copyStatus }}</button>
            </div>
          </div>

          <div class="error-banner" *ngIf="errorMsg">
            <strong>Fetch Error:</strong> {{ errorMsg }}
          </div>

          <textarea 
            *ngIf="responseJson"
            [value]="responseJson" 
            readonly 
            placeholder="Response will be displayed here..." 
            class="api-response-textarea"
            spellcheck="false"
          ></textarea>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .api-client-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    .api-card {
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .card-title {
      font-size: 0.9rem;
    }
    .api-input-row {
      display: flex;
      gap: 0.5rem;
      align-items: center;
    }
    .method-badge {
      background: #22c55e;
      color: white;
      font-weight: bold;
      font-size: 0.75rem;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
    }
    .input-api-url {
      flex: 1;
      background: rgba(30, 41, 59, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 6px;
      padding: 0.4rem 0.75rem;
      color: white;
      font-size: 0.8rem;
      outline: none;
    }
    .input-api-url:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .btn-fetch {
      background: #6366f1;
      border: none;
      color: white;
      padding: 0.4rem 1.25rem;
      border-radius: 6px;
      cursor: pointer;
      font-weight: 600;
      font-size: 0.8rem;
      transition: all 0.2s;
    }
    .btn-fetch:hover:not(:disabled) {
      background: #4f46e5;
    }
    .btn-fetch:disabled {
      background: rgba(99, 102, 241, 0.4);
      cursor: not-allowed;
    }
    .api-response-section {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      padding-top: 1rem;
    }
    .pane-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .pane-title {
      font-size: 0.8rem;
      font-weight: 600;
    }
    .actions-group {
      display: flex;
      gap: 0.5rem;
    }
    .btn-api-action {
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.08);
      color: #cbd5e1;
      padding: 0.25rem 0.65rem;
      border-radius: 4px;
      font-size: 0.75rem;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }
    .btn-api-action:hover {
      background: rgba(255, 255, 255, 0.12);
      color: white;
    }
    .btn-load {
      background: rgba(99, 102, 241, 0.1);
      border-color: rgba(99, 102, 241, 0.2);
      color: #a5b4fc;
    }
    .btn-load:hover {
      background: #6366f1;
      color: white;
    }
    .error-banner {
      background: rgba(239, 68, 68, 0.1);
      border: 1px solid rgba(239, 68, 68, 0.2);
      color: #fca5a5;
      padding: 0.75rem 1rem;
      border-radius: 6px;
      font-size: 0.8rem;
    }
    .api-response-textarea {
      width: 100%;
      height: 350px;
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
  `]
})
export class JsonApiClientComponent {
  @Output() loadJson = new EventEmitter<string>();

  apiUrl: string = '';
  loading: boolean = false;
  responseJson: string = '';
  errorMsg: string = '';
  copyStatus: string = 'Copy Response';

  constructor(private jsonService: JsonExplorerService) {}

  onFetch(): void {
    if (!this.apiUrl) return;
    this.loading = true;
    this.responseJson = '';
    this.errorMsg = '';

    this.jsonService.fetchProxy(this.apiUrl).subscribe({
      next: (res) => {
        // Pretty print the response object
        this.responseJson = JSON.stringify(res, null, 2);
        this.loading = false;
        this.jsonService.saveToHistory(this.responseJson, `Fetch GET ${this.apiUrl.substring(0, 30)}...`);
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = err.error?.error || 'Failed to retrieve response from the requested API URL. Ensure the endpoint is reachable and does not require complex custom credentials.';
      }
    });
  }

  onLoadInEditor(): void {
    if (this.responseJson) {
      this.loadJson.emit(this.responseJson);
    }
  }

  copyResponse(): void {
    if (!this.responseJson) return;
    navigator.clipboard.writeText(this.responseJson).then(() => {
      this.copyStatus = 'Copied!';
      setTimeout(() => {
        this.copyStatus = 'Copy Response';
      }, 2000);
    });
  }
}
