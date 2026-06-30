import { Component } from '@angular/core';

@Component({
  selector: 'app-json-explorer',
  standalone: false,
  template: `
    <div class="explorer-container">
      <!-- Dashboard Header -->
      <div class="explorer-header">
        <div class="title-section">
          <span class="badge">PRO TOOLKIT</span>
          <h1 class="main-title">JSON Explorer</h1>
          <p class="subtitle">A developer-grade suite for formatting, diffing, schema validation, and conversion.</p>
        </div>
      </div>

      <!-- Main Navigation Tabs -->
      <div class="explorer-tabs">
        <button 
          (click)="activeTab = 'editor'" 
          class="tab-btn" 
          [class.active]="activeTab === 'editor'"
        >
          📝 Editor & Tree View
        </button>
        <button 
          (click)="activeTab = 'compare'" 
          class="tab-btn" 
          [class.active]="activeTab === 'compare'"
        >
          ⚖️ JSON Diff Compare
        </button>
        <button 
          (click)="activeTab = 'schema'" 
          class="tab-btn" 
          [class.active]="activeTab === 'schema'"
        >
          📋 Schema & Validate
        </button>
        <button 
          (click)="activeTab = 'converter'" 
          class="tab-btn" 
          [class.active]="activeTab === 'converter'"
        >
          🔄 Format Converter
        </button>
        <button 
          (click)="activeTab = 'api-client'" 
          class="tab-btn" 
          [class.active]="activeTab === 'api-client'"
        >
          🌐 REST response Client
        </button>
        <button 
          (click)="activeTab = 'history'" 
          class="tab-btn" 
          [class.active]="activeTab === 'history'"
        >
          🕒 History & Favorites
        </button>
      </div>

      <!-- Tab Content Area -->
      <div class="explorer-content">
        <!-- Editor & Tree Tab -->
        <div *ngIf="activeTab === 'editor'" class="editor-tree-layout animate-fade-in">
          <div class="layout-column">
            <app-json-editor 
              [initialJson]="currentJson" 
              (jsonUpdated)="onJsonUpdated($event)"
            ></app-json-editor>
          </div>
          <div class="layout-column">
            <app-json-tree 
              [jsonString]="currentJson"
            ></app-json-tree>
          </div>
        </div>

        <!-- Compare/Diff Tab -->
        <div *ngIf="activeTab === 'compare'" class="tab-pane-container animate-fade-in">
          <app-json-compare></app-json-compare>
        </div>

        <!-- Schema Tab -->
        <div *ngIf="activeTab === 'schema'" class="tab-pane-container animate-fade-in">
          <app-json-schema></app-json-schema>
        </div>

        <!-- Converter Tab -->
        <div *ngIf="activeTab === 'converter'" class="tab-pane-container animate-fade-in">
          <app-json-converter></app-json-converter>
        </div>

        <!-- API Client Tab -->
        <div *ngIf="activeTab === 'api-client'" class="tab-pane-container animate-fade-in">
          <app-json-api-client 
            (loadJson)="onLoadJsonFromSource($event)"
          ></app-json-api-client>
        </div>

        <!-- History/Favorites Tab -->
        <div *ngIf="activeTab === 'history'" class="tab-pane-container animate-fade-in">
          <app-json-history 
            (loadJson)="onLoadJsonFromSource($event)"
          ></app-json-history>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .explorer-container {
      max-width: 1200px;
      margin: 1.5rem auto;
      padding: 0 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      color: #cbd5e1;
    }
    .explorer-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: linear-gradient(135deg, rgba(30, 41, 59, 0.4) 0%, rgba(15, 23, 42, 0.4) 100%);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 16px;
      padding: 1.5rem;
      backdrop-filter: blur(12px);
    }
    .badge {
      display: inline-block;
      background: rgba(99, 102, 241, 0.15);
      color: #a5b4fc;
      font-size: 0.7rem;
      font-weight: 700;
      padding: 0.25rem 0.6rem;
      border-radius: 9999px;
      border: 1px solid rgba(99, 102, 241, 0.2);
      letter-spacing: 0.05em;
      margin-bottom: 0.5rem;
    }
    .main-title {
      font-size: 1.75rem;
      font-weight: 800;
      color: #f8fafc;
      letter-spacing: -0.025em;
      margin: 0;
    }
    .subtitle {
      color: #94a3b8;
      font-size: 0.9rem;
      margin: 0.25rem 0 0 0;
    }
    .explorer-tabs {
      display: flex;
      gap: 0.5rem;
      overflow-x: auto;
      padding-bottom: 0.5rem;
      border-bottom: 1px solid rgba(255, 255, 255, 0.08);
    }
    .tab-btn {
      background: transparent;
      border: none;
      color: #94a3b8;
      padding: 0.6rem 1.1rem;
      font-size: 0.9rem;
      font-weight: 600;
      cursor: pointer;
      border-radius: 10px;
      transition: all 0.2s;
      white-space: nowrap;
    }
    .tab-btn:hover {
      color: #cbd5e1;
      background: rgba(255, 255, 255, 0.03);
    }
    .tab-btn.active {
      color: #818cf8;
      background: rgba(99, 102, 241, 0.1);
      border: 1px solid rgba(99, 102, 241, 0.15);
    }
    .explorer-content {
      min-height: 500px;
    }
    .editor-tree-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      align-items: start;
    }
    @media (max-width: 1024px) {
      .editor-tree-layout {
        grid-template-columns: 1fr;
      }
    }
    .layout-column {
      display: flex;
      flex-direction: column;
      height: 100%;
    }
    .tab-pane-container {
      background: rgba(15, 23, 42, 0.3);
      border: 1px solid rgba(255, 255, 255, 0.08);
      border-radius: 16px;
      padding: 1.5rem;
      backdrop-filter: blur(12px);
    }
    .animate-fade-in {
      animation: fadeIn 0.25s ease-out;
    }
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(6px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `]
})
export class JsonExplorerComponent {
  activeTab: 'editor' | 'compare' | 'schema' | 'converter' | 'api-client' | 'history' = 'editor';
  currentJson: string = '{\n  "title": "Welcome to JSON Explorer",\n  "description": "Developer-grade JSON toolkit.",\n  "features": [\n    "Formatter & Tree View",\n    "Diff Compare",\n    "Schema Validate",\n    "Format Conversion",\n    "Code Generation"\n  ],\n  "statistics": {\n    "version": 1.0,\n    "isPremium": true\n  }\n}';

  onJsonUpdated(json: string): void {
    this.currentJson = json;
  }

  onLoadJsonFromSource(json: string): void {
    this.currentJson = json;
    this.activeTab = 'editor';
  }
}
