import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface ValidationResult {
  isValid: boolean;
  errorMessage?: string;
  line?: number;
  column?: number;
  token?: string;
}

export interface DiffLine {
  type: 'added' | 'deleted' | 'modified' | 'unchanged';
  sourceLineNumber?: number;
  targetLineNumber?: number;
  content: string;
}

export interface SchemaValidationResult {
  isValid: boolean;
  errors: string[];
}

export interface JsonStats {
  objectCount: number;
  arrayCount: number;
  keyCount: number;
  maxDepth: number;
  totalSize: number;
}

export interface Snippet {
  id: string;
  label: string;
  json: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class JsonExplorerService {
  private apiUrl = `${environment.apiUrl}/JsonExplorer`;

  constructor(private http: HttpClient) {}

  format(json: string, minify: boolean): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/format`, { json, minify });
  }

  minify(json: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/minify`, { json, minify: true });
  }

  validate(json: string, detectDuplicates: boolean): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, { json, detectDuplicates });
  }

  compare(sourceJson: string, targetJson: string): Observable<DiffLine[]> {
    return this.http.post<DiffLine[]>(`${this.apiUrl}/compare`, { sourceJson, targetJson });
  }

  generateSchema(json: string): Observable<{ schema: string }> {
    return this.http.post<{ schema: string }>(`${this.apiUrl}/schema`, { json });
  }

  validateSchema(dataJson: string, schemaJson: string): Observable<SchemaValidationResult> {
    return this.http.post<SchemaValidationResult>(`${this.apiUrl}/validate-schema`, { dataJson, schemaJson });
  }

  xmlToJson(xml: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/xml-to-json`, { content: xml });
  }

  csvToJson(csv: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/csv-to-json`, { content: csv });
  }

  yamlToJson(yaml: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/yaml-to-json`, { content: yaml });
  }

  excelToJson(file: File): Observable<{ result: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ result: string }>(`${this.apiUrl}/excel-to-json`, formData);
  }

  getStatistics(json: string): Observable<JsonStats> {
    return this.http.post<JsonStats>(`${this.apiUrl}/statistics`, { json });
  }

  flatten(json: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/flatten`, { json });
  }

  unflatten(json: string): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/unflatten`, { json });
  }

  mask(json: string, maskTypes: string[]): Observable<{ result: string }> {
    return this.http.post<{ result: string }>(`${this.apiUrl}/mask`, { json, maskTypes });
  }

  codegen(json: string, language: string, rootObjectName: string): Observable<{ code: string, language: string }> {
    return this.http.post<{ code: string, language: string }>(`${this.apiUrl}/codegen`, { json, language, rootObjectName });
  }

  fetchProxy(url: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/proxy`, { params: { url } });
  }

  // LocalStorage History
  getHistory(): Snippet[] {
    const data = localStorage.getItem('json_explorer_history');
    return data ? JSON.parse(data) : [];
  }

  saveToHistory(json: string, label: string = 'Untitled'): void {
    if (!json || json.trim() === '') return;
    const history = this.getHistory();
    const filtered = history.filter(h => h.json !== json);
    const newSnippet: Snippet = {
      id: Math.random().toString(36).substring(2, 9),
      label: label.substring(0, 50),
      json,
      timestamp: new Date().toISOString()
    };
    filtered.unshift(newSnippet);
    if (filtered.length > 20) {
      filtered.pop();
    }
    localStorage.setItem('json_explorer_history', JSON.stringify(filtered));
  }

  clearHistory(): void {
    localStorage.removeItem('json_explorer_history');
  }

  // LocalStorage Favorites
  getFavorites(): Snippet[] {
    const data = localStorage.getItem('json_explorer_favorites');
    return data ? JSON.parse(data) : [];
  }

  addFavorite(label: string, json: string): void {
    if (!json) return;
    const favorites = this.getFavorites();
    const newFav: Snippet = {
      id: Math.random().toString(36).substring(2, 9),
      label: label || 'My Snippet',
      json,
      timestamp: new Date().toISOString()
    };
    favorites.unshift(newFav);
    localStorage.setItem('json_explorer_favorites', JSON.stringify(favorites));
  }

  removeFavorite(id: string): void {
    const favorites = this.getFavorites();
    const filtered = favorites.filter(f => f.id !== id);
    localStorage.setItem('json_explorer_favorites', JSON.stringify(filtered));
  }
}
