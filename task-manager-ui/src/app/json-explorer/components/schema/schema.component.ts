import { Component } from '@angular/core';
import { JsonExplorerService, SchemaValidationResult } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-schema',
  standalone: false,
  template: `
    <div class="schema-container">
      <div class="tabs-header">
        <button 
          (click)="activeTab = 'generate'" 
          class="tab-btn" 
          [class.active]="activeTab === 'generate'"
        >
          Generate Schema from Sample
        </button>
        <button 
          (click)="activeTab = 'validate'" 
          class="tab-btn" 
          [class.active]="activeTab === 'validate'"
        >
          Validate JSON against Schema
        </button>
      </div>

      <div class="tab-body">
        <!-- Generate Schema Section -->
        <div *ngIf="activeTab === 'generate'" class="generate-section">
          <div class="split-view">
            <div class="pane">
              <span class="pane-title">Sample JSON Input</span>
              <textarea 
                [(ngModel)]="sampleJson" 
                placeholder="Paste sample JSON here..." 
                class="schema-textarea"
                spellcheck="false"
              ></textarea>
              <button 
                (click)="onGenerateSchema()" 
                class="btn-schema-action" 
                [disabled]="loading || !sampleJson"
              >
                {{ loading ? 'Generating...' : 'Generate Schema' }}
              </button>
            </div>
            
            <div class="pane">
              <div class="pane-header">
                <span class="pane-title">Generated JSON Schema</span>
                <button (click)="copySchema()" class="btn-copy-link" *ngIf="generatedSchema">
                  {{ copyStatus }}
                </button>
              </div>
              <textarea 
                [value]="generatedSchema" 
                readonly 
                placeholder="Generated schema will appear here..." 
                class="schema-textarea code-output"
                spellcheck="false"
              ></textarea>
            </div>
          </div>
        </div>

        <!-- Validate Schema Section -->
        <div *ngIf="activeTab === 'validate'" class="validate-section">
          <div class="split-view">
            <div class="pane">
              <div class="pane-header">
                <span class="pane-title">JSON Schema (schema.json)</span>
                <div class="file-upload-wrapper">
                  <label class="btn-file-label">
                    Upload
                    <input type="file" (change)="onUploadSchema($event)" accept=".json" class="hidden-file-input" />
                  </label>
                </div>
              </div>
              <textarea 
                [(ngModel)]="schemaJson" 
                placeholder="Paste or upload JSON Schema here..." 
                class="schema-textarea"
                spellcheck="false"
              ></textarea>
            </div>

            <div class="pane">
              <div class="pane-header">
                <span class="pane-title">JSON Data (data.json)</span>
                <div class="file-upload-wrapper">
                  <label class="btn-file-label">
                    Upload
                    <input type="file" (change)="onUploadData($event)" accept=".json" class="hidden-file-input" />
                  </label>
                </div>
              </div>
              <textarea 
                [(ngModel)]="dataJson" 
                placeholder="Paste or upload JSON Data here..." 
                class="schema-textarea"
                spellcheck="false"
              ></textarea>
            </div>
          </div>

          <div class="validate-action-bar">
            <button 
              (click)="onValidateSchema()" 
              class="btn-schema-action" 
              [disabled]="loading || !schemaJson || !dataJson"
            >
              {{ loading ? 'Validating...' : 'Validate Against Schema' }}
            </button>
          </div>

          <div class="validation-report-card" *ngIf="validationResult">
            <div class="card-header" [class.success]="validationResult.isValid" [class.failure]="!validationResult.isValid">
              <span class="report-badge">{{ validationResult.isValid ? 'PASS' : 'FAIL' }}</span>
              <span class="report-title">
                {{ validationResult.isValid ? 'JSON data conforms to the schema!' : 'Schema validation failed.' }}
              </span>
            </div>
            <div class="card-body" *ngIf="validationResult.errors.length > 0">
              <h4 class="errors-header">Validation Errors ({{ validationResult.errors.length }}):</h4>
              <ul class="errors-list">
                <li *ngFor="let err of validationResult.errors" class="error-item">
                  <code class="error-code-path">{{ err }}</code>
                </li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .schema-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      height: 100%;
    }
    .tabs-header {
      display: flex;
      gap: 0.5rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
      padding-bottom: 0.5rem;
    }
    .tab-btn {
      background: transparent;
      border: none;
      color: #94a3b8;
      padding: 0.5rem 1rem;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      border-radius: 8px;
      transition: all 0.2s;
    }
    .tab-btn:hover {
      color: #cbd5e1;
      background: rgba(255, 255, 255, 0.03);
    }
    .tab-btn.active {
      color: #818cf8;
      background: rgba(99, 102, 241, 0.1);
    }
    .tab-body {
      flex: 1;
    }
    .split-view {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      height: 400px;
    }
    @media (max-width: 768px) {
      .split-view {
        grid-template-columns: 1fr;
        height: auto;
      }
    }
    .pane {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1rem;
    }
    .pane-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .pane-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: #94a3b8;
    }
    .schema-textarea {
      flex: 1;
      min-height: 280px;
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
    .schema-textarea:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .code-output {
      color: #86efac;
    }
    .btn-schema-action {
      background: #6366f1;
      border: none;
      color: white;
      padding: 0.5rem 1.25rem;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 600;
      font-size: 0.85rem;
      transition: all 0.2s;
      align-self: flex-start;
      margin-top: 0.5rem;
    }
    .btn-schema-action:hover:not(:disabled) {
      background: #4f46e5;
    }
    .btn-schema-action:disabled {
      background: rgba(99, 102, 241, 0.4);
      cursor: not-allowed;
    }
    .btn-copy-link {
      background: transparent;
      border: none;
      color: #818cf8;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: bold;
    }
    .btn-copy-link:hover {
      text-decoration: underline;
    }
    .file-upload-wrapper {
      display: flex;
    }
    .btn-file-label {
      background: rgba(255, 255, 255, 0.06);
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #cbd5e1;
      padding: 0.25rem 0.6rem;
      border-radius: 4px;
      font-size: 0.7rem;
      cursor: pointer;
      font-weight: 600;
    }
    .btn-file-label:hover {
      background: rgba(255, 255, 255, 0.12);
    }
    .hidden-file-input {
      display: none;
    }
    .validate-action-bar {
      display: flex;
      justify-content: center;
      margin-top: 1rem;
    }
    .validation-report-card {
      margin-top: 1.5rem;
      border-radius: 12px;
      overflow: hidden;
      border: 1px solid rgba(255, 255, 255, 0.08);
      background: rgba(15, 23, 42, 0.4);
    }
    .card-header {
      padding: 0.75rem 1rem;
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .card-header.success {
      background: rgba(34, 197, 94, 0.1);
      border-bottom: 1px solid rgba(34, 197, 94, 0.2);
    }
    .card-header.failure {
      background: rgba(239, 68, 68, 0.1);
      border-bottom: 1px solid rgba(239, 68, 68, 0.2);
    }
    .report-badge {
      padding: 0.2rem 0.5rem;
      border-radius: 4px;
      font-size: 0.75rem;
      font-weight: 800;
      color: white;
    }
    .success .report-badge { background: #22c55e; }
    .failure .report-badge { background: #ef4444; }
    .report-title {
      font-size: 0.85rem;
      font-weight: 600;
    }
    .success .report-title { color: #86efac; }
    .failure .report-title { color: #fca5a5; }
    .card-body {
      padding: 1rem;
    }
    .errors-header {
      font-size: 0.8rem;
      color: #94a3b8;
      margin-bottom: 0.5rem;
    }
    .errors-list {
      margin: 0;
      padding-left: 1.25rem;
    }
    .error-item {
      color: #fca5a5;
      font-size: 0.8rem;
      margin-bottom: 0.35rem;
    }
    .error-code-path {
      font-family: 'Fira Code', monospace;
      background: rgba(239, 68, 68, 0.05);
      padding: 0.1rem 0.3rem;
      border-radius: 4px;
    }
  `]
})
export class JsonSchemaComponent {
  activeTab: 'generate' | 'validate' = 'generate';
  loading: boolean = false;

  // Generate Mode
  sampleJson: string = '';
  generatedSchema: string = '';
  copyStatus: string = 'Copy Schema';

  // Validate Mode
  schemaJson: string = '';
  dataJson: string = '';
  validationResult: SchemaValidationResult | null = null;

  constructor(private jsonService: JsonExplorerService) {}

  onGenerateSchema(): void {
    if (!this.sampleJson) return;
    this.loading = true;
    this.jsonService.generateSchema(this.sampleJson).subscribe({
      next: (res) => {
        this.generatedSchema = res.schema;
        this.loading = false;
        this.copyStatus = 'Copy Schema';
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to generate JSON Schema.');
      }
    });
  }

  copySchema(): void {
    if (!this.generatedSchema) return;
    navigator.clipboard.writeText(this.generatedSchema).then(() => {
      this.copyStatus = 'Copied!';
      setTimeout(() => {
        this.copyStatus = 'Copy Schema';
      }, 2000);
    });
  }

  onUploadSchema(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.schemaJson = e.target.result;
      };
      reader.readAsText(file);
    }
  }

  onUploadData(event: any): void {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.dataJson = e.target.result;
      };
      reader.readAsText(file);
    }
  }

  onValidateSchema(): void {
    if (!this.schemaJson || !this.dataJson) return;
    this.loading = true;
    this.jsonService.validateSchema(this.dataJson, this.schemaJson).subscribe({
      next: (res) => {
        this.validationResult = res;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to validate JSON against Schema.');
      }
    });
  }
}
