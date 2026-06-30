import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';

export interface TreeNode {
  id: string;
  key: string;
  value: any;
  type: 'object' | 'array' | 'string' | 'number' | 'boolean' | 'null';
  depth: number;
  path: string;
  isExpanded: boolean;
  isVisible: boolean;
  hasChildren: boolean;
  childrenIds?: string[];
  isMatch?: boolean;
}

@Component({
  selector: 'app-json-tree',
  standalone: false,
  template: `
    <div class="tree-container">
      <div class="tree-controls">
        <div class="search-box">
          <input 
            type="text" 
            [(ngModel)]="searchQuery" 
            (ngModelChange)="onSearchChange()" 
            placeholder="Search by key, value, or regex..." 
            class="input-search"
          />
          <button (click)="searchQuery = ''; onSearchChange()" class="btn-clear" *ngIf="searchQuery">×</button>
        </div>
        <div class="btn-group">
          <button (click)="expandAll()" class="btn-action">Expand All</button>
          <button (click)="collapseAll()" class="btn-action">Collapse All</button>
        </div>
      </div>

      <div class="large-file-warning" *ngIf="isTruncated">
        ⚠️ Large file detected ({{ fileSize | number }} bytes). Showing first {{ maxNodes | number }} nodes for smooth performance.
      </div>

      <div class="path-display" *ngIf="selectedPath">
        <span class="path-label">JSONPath:</span> 
        <code class="path-value">{{ selectedPath }}</code>
        <button (click)="copyPath()" class="btn-copy" title="Copy to clipboard">
          {{ copyStatus }}
        </button>
      </div>

      <div class="tree-viewport">
        <div 
          *ngFor="let node of getVisibleNodes()" 
          [style.padding-left.px]="node.depth * 20" 
          class="tree-node"
          [class.matched]="node.isMatch"
          [class.selected]="node.path === selectedPath"
          (click)="onNodeClick(node, $event)"
        >
          <span 
            *ngIf="node.hasChildren" 
            class="toggle-icon" 
            (click)="toggleNode(node, $event)"
          >
            {{ node.isExpanded ? '▼' : '▶' }}
          </span>
          <span *ngIf="!node.hasChildren" class="empty-icon"></span>

          <span class="node-key" [class.highlight]="node.isMatch">{{ node.key }}:</span>

          <ng-container [ngSwitch]="node.type">
            <span *ngSwitchCase="'object'" class="node-bracket">&#123;...&#125;</span>
            <span *ngSwitchCase="'array'" class="node-bracket">[...]</span>
            <span *ngSwitchCase="'string'" class="node-val val-string">"{{ node.value }}"</span>
            <span *ngSwitchCase="'number'" class="node-val val-number">{{ node.value }}</span>
            <span *ngSwitchCase="'boolean'" class="node-val val-boolean">{{ node.value }}</span>
            <span *ngSwitchCase="'null'" class="node-val val-null">null</span>
          </ng-container>
        </div>
        <div *ngIf="allNodes.length === 0" class="empty-state">
          No JSON loaded or JSON is invalid.
        </div>
      </div>
    </div>
  `,
  styles: [`
    .tree-container {
      display: flex;
      flex-direction: column;
      height: 100%;
      background: rgba(15, 23, 42, 0.4);
      border-radius: 12px;
      padding: 1rem;
      border: 1px solid rgba(255, 255, 255, 0.08);
      font-family: 'Fira Code', monospace, Consolas;
    }
    .tree-controls {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1rem;
      flex-wrap: wrap;
    }
    .search-box {
      position: relative;
      flex: 1;
      min-width: 250px;
    }
    .input-search {
      width: 100%;
      background: rgba(30, 41, 59, 0.7);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      padding: 0.5rem 2rem 0.5rem 0.75rem;
      color: #f8fafc;
      font-size: 0.85rem;
      transition: all 0.2s;
    }
    .input-search:focus {
      outline: none;
      border-color: #6366f1;
      box-shadow: 0 0 0 2px rgba(99, 102, 241, 0.2);
    }
    .btn-clear {
      position: absolute;
      right: 8px;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: #94a3b8;
      font-size: 1.15rem;
      cursor: pointer;
      padding: 0 4px;
    }
    .btn-group {
      display: flex;
      gap: 0.5rem;
    }
    .btn-action {
      background: rgba(99, 102, 241, 0.1);
      border: 1px solid rgba(99, 102, 241, 0.2);
      color: #818cf8;
      padding: 0.4rem 0.8rem;
      border-radius: 6px;
      cursor: pointer;
      font-size: 0.8rem;
      font-weight: 500;
      transition: all 0.2s;
    }
    .btn-action:hover {
      background: #6366f1;
      color: white;
    }
    .large-file-warning {
      background: rgba(245, 158, 11, 0.1);
      border: 1px solid rgba(245, 158, 11, 0.2);
      color: #fbbf24;
      padding: 0.6rem 1rem;
      border-radius: 8px;
      font-size: 0.8rem;
      margin-bottom: 0.75rem;
    }
    .path-display {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      background: rgba(99, 102, 241, 0.08);
      border: 1px solid rgba(99, 102, 241, 0.2);
      border-radius: 8px;
      padding: 0.5rem 0.75rem;
      margin-bottom: 1rem;
      font-size: 0.8rem;
    }
    .path-label {
      color: #94a3b8;
      font-weight: bold;
    }
    .path-value {
      color: #818cf8;
      font-family: inherit;
      overflow-x: auto;
      white-space: nowrap;
      flex: 1;
    }
    .btn-copy {
      background: rgba(255, 255, 255, 0.06);
      border: 1px solid rgba(255, 255, 255, 0.1);
      color: #f8fafc;
      padding: 0.25rem 0.6rem;
      border-radius: 4px;
      cursor: pointer;
      font-size: 0.75rem;
      transition: all 0.2s;
    }
    .btn-copy:hover {
      background: rgba(255, 255, 255, 0.15);
    }
    .tree-viewport {
      flex: 1;
      overflow-y: auto;
      max-height: 550px;
      border: 1px solid rgba(255, 255, 255, 0.05);
      background: rgba(15, 23, 42, 0.2);
      border-radius: 8px;
      padding: 0.5rem;
    }
    .tree-node {
      display: flex;
      align-items: center;
      padding: 0.25rem 0;
      cursor: pointer;
      border-radius: 4px;
      font-size: 0.85rem;
      transition: background 0.1s;
    }
    .tree-node:hover {
      background: rgba(255, 255, 255, 0.04);
    }
    .tree-node.selected {
      background: rgba(99, 102, 241, 0.15);
      border-left: 2px solid #6366f1;
    }
    .tree-node.matched {
      background: rgba(234, 179, 8, 0.15);
      border-left: 2px solid #eab308;
    }
    .toggle-icon {
      color: #64748b;
      margin-right: 4px;
      width: 16px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 0.7rem;
      user-select: none;
    }
    .empty-icon {
      width: 20px;
    }
    .node-key {
      color: #93c5fd;
      margin-right: 6px;
      font-weight: 500;
    }
    .node-key.highlight {
      color: #fef08a;
      text-shadow: 0 0 4px rgba(234, 179, 8, 0.3);
    }
    .node-bracket {
      color: #a78bfa;
    }
    .node-val {
      font-weight: normal;
    }
    .val-string { color: #86efac; }
    .val-number { color: #fca5a5; }
    .val-boolean { color: #60a5fa; }
    .val-null { color: #94a3b8; font-style: italic; }
    .empty-state {
      padding: 2rem;
      text-align: center;
      color: #64748b;
      font-size: 0.9rem;
    }
  `]
})
export class JsonTreeComponent implements OnChanges {
  @Input() jsonString: string = '';
  
  allNodes: TreeNode[] = [];
  nodesMap = new Map<string, TreeNode>();
  searchQuery: string = '';
  selectedPath: string = '';
  copyStatus: string = 'Copy';
  isTruncated: boolean = false;
  fileSize: number = 0;
  maxNodes: number = 5000;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['jsonString']) {
      this.selectedPath = '';
      this.rebuildTree();
    }
  }

  rebuildTree(): void {
    this.allNodes = [];
    this.nodesMap.clear();
    this.isTruncated = false;
    this.fileSize = this.jsonString ? new Blob([this.jsonString]).size : 0;

    if (!this.jsonString) return;

    try {
      const parsed = JSON.parse(this.jsonString);
      let count = 0;
      
      const countNodes = (val: any): number => {
        let size = 1;
        const type = this.getType(val);
        if (type === 'object' && val) {
          for (const k of Object.keys(val)) {
            size += countNodes(val[k]);
          }
        } else if (type === 'array' && val) {
          for (const item of val) {
            size += countNodes(item);
          }
        }
        return size;
      };

      const nodeCount = countNodes(parsed);
      if (nodeCount > this.maxNodes) {
        this.isTruncated = true;
      }

      this.buildTreeNodes(parsed, 'Root', 0, '$');
    } catch {
      // JSON is invalid, tree empty
    }
  }

  buildTreeNodes(val: any, key: string, depth: number, path: string): string[] {
    if (this.allNodes.length >= this.maxNodes) {
      return [];
    }

    const id = Math.random().toString(36).substring(2, 9);
    const type = this.getType(val);
    const node: TreeNode = {
      id,
      key,
      value: val,
      type,
      depth,
      path,
      isExpanded: depth < 2,
      isVisible: depth <= 1,
      hasChildren: type === 'object' || type === 'array'
    };

    this.allNodes.push(node);
    this.nodesMap.set(id, node);

    if (node.hasChildren && val) {
      const childrenIds: string[] = [];
      if (type === 'object') {
        for (const k of Object.keys(val)) {
          const childPath = path === '$' ? `$.${k}` : `${path}.${k}`;
          const cIds = this.buildTreeNodes(val[k], k, depth + 1, childPath);
          childrenIds.push(...cIds);
        }
      } else if (type === 'array') {
        let idx = 0;
        for (const item of val) {
          const childPath = `${path}[${idx}]`;
          const cIds = this.buildTreeNodes(item, `[${idx}]`, depth + 1, childPath);
          childrenIds.push(...cIds);
          idx++;
        }
      }
      node.childrenIds = childrenIds;
    }

    return [id];
  }

  getType(val: any): 'object' | 'array' | 'string' | 'number' | 'boolean' | 'null' {
    if (val === null) return 'null';
    if (Array.isArray(val)) return 'array';
    const type = typeof val;
    if (type === 'string' || type === 'number' || type === 'boolean') {
      return type as any;
    }
    return 'object';
  }

  getVisibleNodes(): TreeNode[] {
    return this.allNodes.filter(n => n.isVisible);
  }

  toggleNode(node: TreeNode, event: Event): void {
    event.stopPropagation();
    node.isExpanded = !node.isExpanded;
    this.updateVisibility();
  }

  updateVisibility(): void {
    const setChildrenVisibility = (n: TreeNode, visible: boolean) => {
      if (!n.childrenIds) return;
      for (const childId of n.childrenIds) {
        const child = this.nodesMap.get(childId);
        if (child) {
          child.isVisible = visible;
          // Only show child's children if the child itself is expanded and parent is visible
          setChildrenVisibility(child, visible && child.isExpanded);
        }
      }
    };

    // Reset visibility based on expansion
    for (const node of this.allNodes) {
      if (node.depth === 0) {
        node.isVisible = true;
        setChildrenVisibility(node, node.isExpanded);
      }
    }
  }

  onNodeClick(node: TreeNode, event: Event): void {
    event.stopPropagation();
    this.selectedPath = node.path;
    this.copyStatus = 'Copy';
  }

  copyPath(): void {
    if (!this.selectedPath) return;
    navigator.clipboard.writeText(this.selectedPath).then(() => {
      this.copyStatus = 'Copied!';
      setTimeout(() => {
        this.copyStatus = 'Copy';
      }, 2000);
    });
  }

  expandAll(): void {
    for (const node of this.allNodes) {
      if (node.hasChildren) {
        node.isExpanded = true;
      }
      node.isVisible = true;
    }
  }

  collapseAll(): void {
    for (const node of this.allNodes) {
      if (node.hasChildren) {
        node.isExpanded = false;
      }
      node.isVisible = node.depth <= 1;
    }
  }

  onSearchChange(): void {
    const query = this.searchQuery.trim().toLowerCase();
    if (!query) {
      for (const node of this.allNodes) {
        node.isMatch = false;
      }
      this.updateVisibility();
      return;
    }

    let isRegex = false;
    let regex: RegExp | null = null;
    if (query.startsWith('/') && (query.endsWith('/') || query.substring(1).includes('/'))) {
      try {
        const lastSlash = query.lastIndexOf('/');
        const pattern = query.substring(1, lastSlash);
        const flags = query.substring(lastSlash + 1);
        regex = new RegExp(pattern, flags);
        isRegex = true;
      } catch {
        regex = null;
      }
    }

    const matches: TreeNode[] = [];

    for (const node of this.allNodes) {
      let isMatch = false;
      const keyString = String(node.key).toLowerCase();
      const valString = node.value !== null && typeof node.value !== 'object' ? String(node.value).toLowerCase() : '';

      if (isRegex && regex) {
        isMatch = regex.test(node.key) || (valString !== '' && regex.test(String(node.value)));
      } else {
        isMatch = keyString.includes(query) || valString.includes(query);
      }

      node.isMatch = isMatch;
      if (isMatch) {
        matches.push(node);
      }
    }

    // Auto-expand parents of matches
    if (matches.length > 0) {
      for (const node of matches) {
        this.expandParents(node);
      }
      this.updateVisibility();
    }
  }

  expandParents(node: TreeNode): void {
    // Traverse parent nodes
    const parts = node.path.split('.');
    let currentPath = '$';
    
    // To expand parents, find nodes whose path matches prefixes of the current node's path
    for (const nodeItem of this.allNodes) {
      if (nodeItem.hasChildren && node.path.startsWith(nodeItem.path) && node.path !== nodeItem.path) {
        nodeItem.isExpanded = true;
        nodeItem.isVisible = true;
      }
    }
  }
}
