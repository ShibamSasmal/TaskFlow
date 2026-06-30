import { Component, EventEmitter, Input, Output } from '@angular/core';
import { JsonExplorerService, JsonStats, ValidationResult } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-editor',
  standalone: false,
  template: `
    <div class="editor-view-container">
      <!-- Main Text Editor Panel -->
      <div class="editor-main-card">
        <div class="editor-header">
          <span class="panel-title">Raw JSON Input</span>
          <div class="editor-actions">
            <button (click)="onFormat()" class="btn-util btn-primary-accent" [disabled]="loading">Format</button>
            <button (click)="onMinify()" class="btn-util" [disabled]="loading">Minify</button>
            <button (click)="onClear()" class="btn-util btn-danger-accent" [disabled]="loading">Clear</button>
            <button (click)="onSaveFavorite()" class="btn-util btn-fav" [disabled]="!jsonText || loading">⭐ Save Snippet</button>
          </div>
        </div>

        <div class="textarea-wrapper">
          <textarea 
            [(ngModel)]="jsonText" 
            (ngModelChange)="onTextChange()" 
            placeholder="Paste your raw JSON document here..." 
            class="json-textarea"
            spellcheck="false"
          ></textarea>
        </div>

        <div class="validation-status" *ngIf="validation">
          <div class="status-indicator" [class.valid]="validation.isValid" [class.invalid]="!validation.isValid">
            <span class="status-dot"></span>
            <span class="status-msg">
              {{ validation.isValid ? 'Valid JSON Syntax' : (validation.errorMessage || 'Invalid JSON syntax') }}
              <span *ngIf="validation.line !== undefined && validation.line !== null" class="status-position">
                (Line: {{ validation.line }}, Col: {{ validation.column }})
              </span>
            </span>
          </div>
        </div>
      </div>

      <!-- Tools & Settings Grid (Stacked Below the Text Editor) -->
      <div class="editor-tools-grid">
        <!-- Box 1: Transformations & Masking -->
        <div class="tool-card">
          <h3 class="card-title">Transformations & Masking</h3>
          <div class="transform-actions">
            <button (click)="onFlatten()" class="btn-op" [disabled]="!jsonText || loading">Flatten JSON</button>
            <button (click)="onUnflatten()" class="btn-op" [disabled]="!jsonText || loading">Unflatten JSON</button>
          </div>
          
          <div class="mask-section">
            <span class="section-subtitle">Mask Sensitive Fields</span>
            <div class="mask-checkboxes">
              <label class="check-container">
                <input type="checkbox" [(ngModel)]="maskEmails" />
                <span class="checkmark"></span>
                <span class="label-text">Mask Emails</span>
              </label>
              <label class="check-container">
                <input type="checkbox" [(ngModel)]="maskPhones" />
                <span class="checkmark"></span>
                <span class="label-text">Mask Phone Numbers</span>
              </label>
              <label class="check-container">
                <input type="checkbox" [(ngModel)]="maskCards" />
                <span class="checkmark"></span>
                <span class="label-text">Mask Credit Cards</span>
              </label>
            </div>
            <button (click)="onApplyMask()" class="btn-mask" [disabled]="!jsonText || loading">Apply Masking Filters</button>
          </div>
        </div>

        <!-- Box 2: Code Generator -->
        <div class="tool-card">
          <h3 class="card-title">Code Generator</h3>
          <div class="codegen-setup">
            <div class="input-row">
              <input type="text" [(ngModel)]="rootName" placeholder="Root class name..." class="input-text" />
              <select [(ngModel)]="selectedLanguage" class="select-lang">
                <option value="csharp">C# Class</option>
                <option value="typescript">TypeScript Interface</option>
                <option value="sql">SQL CREATE TABLE</option>
              </select>
            </div>
            <button (click)="onGenerateCode()" class="btn-generate" [disabled]="!jsonText || loading">Generate Source Code</button>
          </div>

          <div class="generated-code-box" *ngIf="generatedCode">
            <div class="code-box-header">
              <span class="code-box-lang">{{ selectedLanguage | uppercase }} Output</span>
              <button (click)="copyGeneratedCode()" class="btn-copy-code">{{ copyCodeStatus }}</button>
            </div>
            <pre class="code-pre"><code>{{ generatedCode }}</code></pre>
          </div>
        </div>

        <!-- Box 3: Structural Statistics (Full Width Span) -->
        <div class="tool-card span-2" *ngIf="stats">
          <h3 class="card-title">Structural Statistics</h3>
          <div class="stats-grid">
            <div class="stat-item">
              <span class="stat-val">{{ stats.objectCount | number }}</span>
              <span class="stat-lbl">Objects</span>
            </div>
            <div class="stat-item">
              <span class="stat-val">{{ stats.arrayCount | number }}</span>
              <span class="stat-lbl">Arrays</span>
            </div>
            <div class="stat-item">
              <span class="stat-val">{{ stats.keyCount | number }}</span>
              <span class="stat-lbl">Keys</span>
            </div>
            <div class="stat-item">
              <span class="stat-val">{{ stats.maxDepth | number }}</span>
              <span class="stat-lbl">Max Depth</span>
            </div>
            <div class="stat-item">
              <span class="stat-val">{{ formatBytes(stats.totalSize) }}</span>
              <span class="stat-lbl">Total Size</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .editor-view-container {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      height: 100%;
    }
    .editor-main-card {
      display: flex;
      flex-direction: column;
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
    }
    .editor-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.75rem;
      flex-wrap: wrap;
      gap: 0.5rem;
    }
    .panel-title {
      font-weight: 600;
      color: #f8fafc;
      font-size: 0.95rem;
    }
    .editor-actions {
      display: flex;
      gap: 0.5rem;
    }
    .btn-util {
      background: rgba(255, 255, 255, 0.06);
      border: 1px solid rgba(255, 255, 255, 0.08);
      color: #cbd5e1;
      padding: 0.35rem 0.75rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.8rem;
      font-weight: 500;
      transition: all 0.2s;
    }
    .btn-util:hover {
      background: rgba(255, 255, 255, 0.12);
      color: white;
    }
    .btn-primary-accent {
      background: #6366f1;
      border-color: #6366f1;
      color: white;
    }
    .btn-primary-accent:hover {
      background: #4f46e5;
    }
    .btn-danger-accent {
      color: #f87171;
      border-color: rgba(239, 68, 68, 0.2);
    }
    .btn-danger-accent:hover {
      background: rgba(239, 68, 68, 0.15);
      color: white;
    }
    .btn-fav {
      border-color: rgba(234, 179, 8, 0.2);
      color: #fbbf24;
    }
    .btn-fav:hover {
      background: rgba(234, 179, 8, 0.1);
    }
    .textarea-wrapper {
      flex: 1;
      min-height: 380px;
      margin-bottom: 0.75rem;
    }
    .json-textarea {
      width: 100%;
      height: 100%;
      min-height: 380px;
      background: rgba(15, 23, 42, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.05);
      border-radius: 8px;
      padding: 0.75rem;
      color: #38bdf8;
      font-family: 'Fira Code', monospace, Consolas, Courier;
      font-size: 0.85rem;
      line-height: 1.5;
      resize: vertical;
      outline: none;
      transition: border 0.2s;
    }
    .json-textarea:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .validation-status {
      padding: 0.5rem;
      border-radius: 6px;
      background: rgba(15, 23, 42, 0.3);
    }
    .status-indicator {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.8rem;
    }
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    .valid .status-dot {
      background: #22c55e;
      box-shadow: 0 0 8px #22c55e;
    }
    .invalid .status-dot {
      background: #ef4444;
      box-shadow: 0 0 8px #ef4444;
    }
    .valid .status-msg { color: #86efac; }
    .invalid .status-msg { color: #fca5a5; }
    .status-position {
      color: #94a3b8;
      font-weight: 500;
      margin-left: 0.5rem;
    }

    /* Tools & Options Settings */
    .editor-tools-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    @media (max-width: 640px) {
      .editor-tools-grid {
        grid-template-columns: 1fr;
      }
      .span-2 {
        grid-column: span 1 !important;
      }
    }
    .tool-card {
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }
    .span-2 {
      grid-column: span 2;
    }
    .card-title {
      font-size: 0.85rem;
      font-weight: 600;
      color: #94a3b8;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      margin-bottom: 0.25rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
      padding-bottom: 0.4rem;
    }
    
    .transform-actions {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 0.5rem;
    }
    .btn-op {
      background: rgba(255, 255, 255, 0.04);
      border: 1px solid rgba(255, 255, 255, 0.08);
      color: #cbd5e1;
      padding: 0.5rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 500;
      transition: all 0.2s;
    }
    .btn-op:hover:not(:disabled) {
      background: rgba(99, 102, 241, 0.1);
      color: #818cf8;
      border-color: rgba(99, 102, 241, 0.2);
    }
    .mask-section {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      padding-top: 0.75rem;
    }
    .section-subtitle {
      font-size: 0.75rem;
      font-weight: 700;
      color: #64748b;
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .mask-checkboxes {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    /* Custom Checkboxes */
    .check-container {
      display: flex;
      align-items: center;
      position: relative;
      cursor: pointer;
      font-size: 0.8rem;
      color: #cbd5e1;
      user-select: none;
      gap: 0.6rem;
      padding: 0.2rem 0;
    }
    .check-container input {
      position: absolute;
      opacity: 0;
      cursor: pointer;
      height: 0;
      width: 0;
    }
    .checkmark {
      height: 18px;
      width: 18px;
      background-color: rgba(30, 41, 59, 0.8);
      border: 1px solid rgba(255, 255, 255, 0.15);
      border-radius: 4px;
      transition: all 0.2s;
      display: inline-block;
      flex-shrink: 0;
      position: relative;
    }
    .check-container:hover input ~ .checkmark {
      border-color: #6366f1;
      background-color: rgba(99, 102, 241, 0.05);
    }
    .check-container input:checked ~ .checkmark {
      background-color: #6366f1;
      border-color: #6366f1;
      box-shadow: 0 0 8px rgba(99, 102, 241, 0.4);
    }
    .checkmark:after {
      content: "";
      position: absolute;
      display: none;
      left: 6px;
      top: 2px;
      width: 5px;
      height: 9px;
      border: solid white;
      border-width: 0 2px 2px 0;
      transform: rotate(45deg);
    }
    .check-container input:checked ~ .checkmark:after {
      display: block;
    }
    .label-text {
      font-weight: 500;
    }

    .btn-mask {
      background: rgba(168, 85, 247, 0.1);
      border: 1px solid rgba(168, 85, 247, 0.2);
      color: #c084fc;
      padding: 0.45rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 600;
      transition: all 0.2s;
    }
    .btn-mask:hover:not(:disabled) {
      background: #a855f7;
      color: white;
    }

    .codegen-setup {
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }
    .input-row {
      display: grid;
      grid-template-columns: 1.2fr 0.8fr;
      gap: 0.5rem;
    }
    .input-text {
      background: rgba(30, 41, 59, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 6px;
      padding: 0.4rem 0.5rem;
      color: white;
      font-size: 0.75rem;
      outline: none;
    }
    .input-text:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .select-lang {
      background: rgba(30, 41, 59, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 6px;
      padding: 0.4rem 0.5rem;
      color: white;
      font-size: 0.75rem;
      outline: none;
    }
    .btn-generate {
      background: rgba(99, 102, 241, 0.1);
      border: 1px solid rgba(99, 102, 241, 0.2);
      color: #818cf8;
      padding: 0.45rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 600;
      transition: all 0.2s;
    }
    .btn-generate:hover:not(:disabled) {
      background: #6366f1;
      color: white;
    }
    .generated-code-box {
      margin-top: 0.75rem;
      border: 1px solid rgba(255, 255, 255, 0.05);
      background: rgba(15, 23, 42, 0.5);
      border-radius: 8px;
    }
    .code-box-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 0.4rem 0.6rem;
      background: rgba(255, 255, 255, 0.03);
      border-bottom: 1px solid rgba(255, 255, 255, 0.05);
    }
    .code-box-lang {
      font-size: 0.7rem;
      font-weight: bold;
      color: #94a3b8;
    }
    .btn-copy-code {
      background: transparent;
      border: none;
      color: #6366f1;
      font-size: 0.7rem;
      cursor: pointer;
      font-weight: bold;
    }
    .btn-copy-code:hover {
      text-decoration: underline;
    }
    .code-pre {
      margin: 0;
      padding: 0.6rem;
      font-size: 0.75rem;
      max-height: 180px;
      overflow-y: auto;
      font-family: 'Fira Code', monospace;
      color: #cbd5e1;
      white-space: pre-wrap;
    }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(5, 1fr);
      gap: 0.75rem;
    }
    @media (max-width: 768px) {
      .stats-grid {
        grid-template-columns: repeat(2, 1fr);
      }
    }
    .stat-item {
      background: rgba(255, 255, 255, 0.02);
      border: 1px solid rgba(255, 255, 255, 0.04);
      padding: 0.5rem;
      border-radius: 8px;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
    }
    .stat-val {
      font-size: 1.15rem;
      font-weight: bold;
      color: #38bdf8;
      font-family: 'Fira Code', monospace;
    }
    .stat-lbl {
      font-size: 0.7rem;
      color: #64748b;
      text-transform: uppercase;
      font-weight: 600;
      margin-top: 0.2rem;
    }
  `]
})
export class JsonEditorComponent {
  @Input() set initialJson(val: string) {
    if (val !== undefined && val !== null) {
      this.jsonText = val;
      this.onTextChange();
    }
  }
  @Output() jsonUpdated = new EventEmitter<string>();

  jsonText: string = '';
  loading: boolean = false;
  validation: ValidationResult | null = null;
  stats: JsonStats | null = null;

  // Masking
  maskEmails: boolean = false;
  maskPhones: boolean = false;
  maskCards: boolean = false;

  // CodeGen
  rootName: string = 'RootObject';
  selectedLanguage: string = 'csharp';
  generatedCode: string = '';
  copyCodeStatus: string = 'Copy';

  constructor(private jsonService: JsonExplorerService) {}

  onTextChange(): void {
    this.jsonUpdated.emit(this.jsonText);
    this.runValidationAndStats();
  }

  runValidationAndStats(): void {
    if (!this.jsonText || this.jsonText.trim() === '') {
      this.validation = null;
      this.stats = null;
      return;
    }

    this.jsonService.validate(this.jsonText, true).subscribe({
      next: (res) => {
        this.validation = res;
        if (res.isValid) {
          this.jsonService.getStatistics(this.jsonText).subscribe({
            next: (s) => this.stats = s
          });
        } else {
          this.stats = null;
        }
      },
      error: () => {
        this.validation = { isValid: false, errorMessage: 'Failed to communicate with validator API.' };
        this.stats = null;
      }
    });
  }

  onFormat(): void {
    if (!this.jsonText) return;
    this.loading = true;
    this.jsonService.format(this.jsonText, false).subscribe({
      next: (res) => {
        this.jsonText = res.result;
        this.onTextChange();
        this.loading = false;
        this.jsonService.saveToHistory(this.jsonText, 'Format JSON');
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to format JSON.');
      }
    });
  }

  onMinify(): void {
    if (!this.jsonText) return;
    this.loading = true;
    this.jsonService.minify(this.jsonText).subscribe({
      next: (res) => {
        this.jsonText = res.result;
        this.onTextChange();
        this.loading = false;
        this.jsonService.saveToHistory(this.jsonText, 'Minify JSON');
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to minify JSON.');
      }
    });
  }

  onClear(): void {
    this.jsonText = '';
    this.validation = null;
    this.stats = null;
    this.generatedCode = '';
    this.jsonUpdated.emit('');
  }

  onSaveFavorite(): void {
    if (!this.jsonText) return;
    const label = prompt('Enter a label for this snippet:', 'Snippet ' + new Date().toLocaleTimeString());
    if (label !== null) {
      this.jsonService.addFavorite(label, this.jsonText);
      alert('Snippet saved to Favorites!');
    }
  }

  onFlatten(): void {
    if (!this.jsonText) return;
    this.loading = true;
    this.jsonService.flatten(this.jsonText).subscribe({
      next: (res) => {
        this.jsonText = res.result;
        this.onTextChange();
        this.loading = false;
        this.jsonService.saveToHistory(this.jsonText, 'Flatten JSON');
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to flatten JSON.');
      }
    });
  }

  onUnflatten(): void {
    if (!this.jsonText) return;
    this.loading = true;
    this.jsonService.unflatten(this.jsonText).subscribe({
      next: (res) => {
        this.jsonText = res.result;
        this.onTextChange();
        this.loading = false;
        this.jsonService.saveToHistory(this.jsonText, 'Unflatten JSON');
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to unflatten JSON.');
      }
    });
  }

  onApplyMask(): void {
    if (!this.jsonText) return;
    const maskTypes: string[] = [];
    if (this.maskEmails) maskTypes.push('email');
    if (this.maskPhones) maskTypes.push('phone');
    if (this.maskCards) maskTypes.push('card');

    if (maskTypes.length === 0) {
      alert('Please select at least one masking category.');
      return;
    }

    this.loading = true;
    this.jsonService.mask(this.jsonText, maskTypes).subscribe({
      next: (res) => {
        this.jsonText = res.result;
        this.onTextChange();
        this.loading = false;
        this.jsonService.saveToHistory(this.jsonText, 'Mask Sensitive Data');
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to apply masking.');
      }
    });
  }

  onGenerateCode(): void {
    if (!this.jsonText) return;
    this.loading = true;
    this.jsonService.codegen(this.jsonText, this.selectedLanguage, this.rootName || 'RootObject').subscribe({
      next: (res) => {
        this.generatedCode = res.code;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || 'Failed to generate code.');
      }
    });
  }

  copyGeneratedCode(): void {
    if (!this.generatedCode) return;
    navigator.clipboard.writeText(this.generatedCode).then(() => {
      this.copyCodeStatus = 'Copied!';
      setTimeout(() => {
        this.copyCodeStatus = 'Copy';
      }, 2000);
    });
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
