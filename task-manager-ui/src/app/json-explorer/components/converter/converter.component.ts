import { Component } from '@angular/core';
import { JsonExplorerService } from '../../services/json-explorer.service';

@Component({
  selector: 'app-json-converter',
  standalone: false,
  template: `
    <div class="converter-container">
      <div class="converter-layout">
        <div class="convert-pane">
          <span class="pane-title font-semibold text-slate-200">Import & Convert</span>
          
          <div class="converter-settings">
            <label class="select-label">Conversion Type:</label>
            <select [(ngModel)]="conversionType" class="select-format" (change)="onTypeChange()">
              <option value="csv">CSV to JSON</option>
              <option value="xml">XML to JSON</option>
              <option value="yaml">YAML to JSON</option>
              <option value="excel">Excel (.xlsx) to JSON</option>
            </select>
          </div>

          <!-- File Upload Zone -->
          <div class="upload-zone" (dragover)="onDragOver($event)" (drop)="onDrop($event)">
            <span class="upload-icon">📁</span>
            <span class="upload-text">Drag and drop file here, or click to browse</span>
            <span class="file-limits">Supports .csv, .xml, .yaml, .yml, .xlsx</span>
            <input 
              type="file" 
              (change)="onFileSelected($event)" 
              [accept]="getFileAccept()" 
              class="hidden-file-input" 
              #fileInput 
            />
            <button (click)="fileInput.click()" class="btn-browse">Browse Files</button>
            <div class="selected-file" *ngIf="selectedFile">
              Selected: <strong class="file-name">{{ selectedFile.name }}</strong> ({{ formatBytes(selectedFile.size) }})
            </div>
          </div>

          <!-- Text input zone for CSV, XML, YAML -->
          <div class="text-input-section" *ngIf="conversionType !== 'excel'">
            <span class="or-separator">--- OR PASTE RAW TEXT ---</span>
            <textarea 
              [(ngModel)]="rawText" 
              [placeholder]="getPlaceholder()" 
              class="converter-textarea"
              spellcheck="false"
            ></textarea>
          </div>

          <button 
            (click)="onConvert()" 
            class="btn-convert-action" 
            [disabled]="loading || (!selectedFile && !rawText && conversionType !== 'excel')"
          >
            {{ loading ? 'Converting...' : 'Convert to JSON' }}
          </button>
        </div>

        <div class="convert-pane">
          <div class="pane-header">
            <span class="pane-title font-semibold text-slate-200">Converted JSON Result</span>
            <div class="result-actions" *ngIf="convertedResult">
              <button (click)="copyResult()" class="btn-result-action">{{ copyStatus }}</button>
              <button (click)="downloadResult()" class="btn-result-action">Download</button>
            </div>
          </div>
          <textarea 
            [value]="convertedResult" 
            readonly 
            placeholder="Converted JSON will be displayed here..." 
            class="converter-textarea result-textarea"
            spellcheck="false"
          ></textarea>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .converter-container {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    .converter-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
    }
    @media (max-width: 968px) {
      .converter-layout {
        grid-template-columns: 1fr;
      }
    }
    .convert-pane {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      background: rgba(15, 23, 42, 0.4);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 12px;
      padding: 1.25rem;
    }
    .pane-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .pane-title {
      font-size: 0.9rem;
    }
    .converter-settings {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .select-label {
      font-size: 0.8rem;
      color: #94a3b8;
      font-weight: 500;
    }
    .select-format {
      background: rgba(30, 41, 59, 0.6);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 6px;
      padding: 0.35rem 0.5rem;
      color: white;
      font-size: 0.8rem;
      outline: none;
    }
    .upload-zone {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      border: 2px dashed rgba(255, 255, 255, 0.08);
      border-radius: 8px;
      padding: 1.5rem;
      background: rgba(255, 255, 255, 0.01);
      cursor: pointer;
      text-align: center;
      transition: all 0.2s;
    }
    .upload-zone:hover {
      border-color: rgba(99, 102, 241, 0.4);
      background: rgba(99, 102, 241, 0.02);
    }
    .upload-icon {
      font-size: 2rem;
      margin-bottom: 0.5rem;
    }
    .upload-text {
      font-size: 0.85rem;
      color: #cbd5e1;
      font-weight: 500;
    }
    .file-limits {
      font-size: 0.7rem;
      color: #64748b;
      margin-bottom: 0.75rem;
    }
    .hidden-file-input {
      display: none;
    }
    .btn-browse {
      background: rgba(99, 102, 241, 0.1);
      border: 1px solid rgba(99, 102, 241, 0.2);
      color: #818cf8;
      padding: 0.35rem 0.75rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.75rem;
      font-weight: 600;
      transition: all 0.2s;
    }
    .btn-browse:hover {
      background: #6366f1;
      color: white;
    }
    .selected-file {
      margin-top: 0.75rem;
      font-size: 0.75rem;
      color: #a78bfa;
    }
    .file-name {
      color: white;
    }
    .or-separator {
      display: block;
      text-align: center;
      font-size: 0.7rem;
      color: #64748b;
      letter-spacing: 0.1em;
      margin: 0.5rem 0;
    }
    .converter-textarea {
      width: 100%;
      height: 200px;
      background: rgba(15, 23, 42, 0.5);
      border: 1px solid rgba(255, 255, 255, 0.05);
      border-radius: 8px;
      padding: 0.75rem;
      color: #e2e8f0;
      font-family: 'Fira Code', monospace;
      font-size: 0.8rem;
      line-height: 1.5;
      resize: vertical;
      outline: none;
    }
    .converter-textarea:focus {
      border-color: rgba(99, 102, 241, 0.4);
    }
    .result-textarea {
      height: 420px;
      color: #86efac;
      resize: none;
    }
    .btn-convert-action {
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
    }
    .btn-convert-action:hover:not(:disabled) {
      background: #4f46e5;
    }
    .btn-convert-action:disabled {
      background: rgba(99, 102, 241, 0.4);
      cursor: not-allowed;
    }
    .result-actions {
      display: flex;
      gap: 0.5rem;
    }
    .btn-result-action {
      background: rgba(255, 255, 255, 0.06);
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #cbd5e1;
      padding: 0.25rem 0.65rem;
      border-radius: 6px;
      font-size: 0.75rem;
      cursor: pointer;
      font-weight: 500;
      transition: all 0.2s;
    }
    .btn-result-action:hover {
      background: rgba(255, 255, 255, 0.12);
      color: white;
    }
  `]
})
export class JsonConverterComponent {
  conversionType: 'csv' | 'xml' | 'yaml' | 'excel' = 'csv';
  selectedFile: File | null = null;
  rawText: string = '';
  convertedResult: string = '';
  loading: boolean = false;
  copyStatus: string = 'Copy';

  constructor(private jsonService: JsonExplorerService) {}

  onTypeChange(): void {
    this.selectedFile = null;
    this.rawText = '';
  }

  getPlaceholder(): string {
    switch (this.conversionType) {
      case 'csv':
        return 'id,name,email\n1,John Doe,john@example.com\n2,Jane Smith,jane@example.com';
      case 'xml':
        return '<employees>\n  <employee>\n    <id>1</id>\n    <name>John Doe</name>\n  </employee>\n</employees>';
      case 'yaml':
        return 'employees:\n  - id: 1\n    name: John Doe\n  - id: 2\n    name: Jane Smith';
      default:
        return '';
    }
  }

  getFileAccept(): string {
    switch (this.conversionType) {
      case 'csv': return '.csv';
      case 'xml': return '.xml';
      case 'yaml': return '.yaml,.yml';
      case 'excel': return '.xlsx';
    }
  }

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.selectedFile = file;
      this.rawText = ''; // Clear text if file is uploaded
    }
  }

  onDragOver(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onDrop(event: any): void {
    event.preventDefault();
    event.stopPropagation();
    const file = event.dataTransfer.files[0];
    if (file) {
      // Validate file type
      const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
      const accepts = this.getFileAccept().split(',');
      if (accepts.includes(ext) || this.conversionType === 'yaml' && ext === '.yml') {
        this.selectedFile = file;
        this.rawText = '';
      } else {
        alert(`Invalid file type. Please upload a ${this.getFileAccept()} file.`);
      }
    }
  }

  onConvert(): void {
    if (this.conversionType === 'excel' && !this.selectedFile) {
      alert('Please upload an Excel (.xlsx) file first.');
      return;
    }

    this.loading = true;
    this.convertedResult = '';

    if (this.selectedFile) {
      // Convert via file upload
      if (this.conversionType === 'excel') {
        this.jsonService.excelToJson(this.selectedFile).subscribe({
          next: (res) => {
            this.convertedResult = res.result;
            this.loading = false;
            this.jsonService.saveToHistory(this.convertedResult, 'Excel to JSON');
          },
          error: (err) => {
            this.loading = false;
            alert(err.error?.error || 'Failed to convert Excel sheet.');
          }
        });
      } else {
        // Read file contents as text and convert
        const reader = new FileReader();
        reader.onload = (e: any) => {
          const text = e.target.result;
          this.convertText(text);
        };
        reader.readAsText(this.selectedFile);
      }
    } else if (this.rawText) {
      this.convertText(this.rawText);
    }
  }

  convertText(text: string): void {
    let obs$;
    switch (this.conversionType) {
      case 'csv':
        obs$ = this.jsonService.csvToJson(text);
        break;
      case 'xml':
        obs$ = this.jsonService.xmlToJson(text);
        break;
      case 'yaml':
        obs$ = this.jsonService.yamlToJson(text);
        break;
      default:
        this.loading = false;
        return;
    }

    obs$.subscribe({
      next: (res) => {
        this.convertedResult = res.result;
        this.loading = false;
        this.jsonService.saveToHistory(this.convertedResult, `${this.conversionType.toUpperCase()} to JSON`);
      },
      error: (err) => {
        this.loading = false;
        alert(err.error?.error || `Failed to convert ${this.conversionType.toUpperCase()} content.`);
      }
    });
  }

  copyResult(): void {
    if (!this.convertedResult) return;
    navigator.clipboard.writeText(this.convertedResult).then(() => {
      this.copyStatus = 'Copied!';
      setTimeout(() => {
        this.copyStatus = 'Copy';
      }, 2000);
    });
  }

  downloadResult(): void {
    if (!this.convertedResult) return;
    const blob = new Blob([this.convertedResult], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `converted_${Date.now()}.json`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
