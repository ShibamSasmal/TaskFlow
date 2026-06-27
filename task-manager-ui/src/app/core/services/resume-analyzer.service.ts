import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AnalysisResponse, AnalysisSummary } from '../models/resume-analysis.model';

@Injectable({
  providedIn: 'root'
})
export class ResumeAnalyzerService {
  private apiUrl = `${environment.apiUrl}/resume`;

  constructor(private http: HttpClient) {}

  uploadAndAnalyze(file: File, targetRole: string): Observable<AnalysisResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('targetRole', targetRole);

    return this.http.post<AnalysisResponse>(`${this.apiUrl}/analyze`, formData);
  }

  getRoles(): Observable<{ roles: string[] }> {
    return this.http.get<{ roles: string[] }>(`${this.apiUrl}/roles`);
  }

  getHistory(): Observable<AnalysisSummary[]> {
    return this.http.get<AnalysisSummary[]>(`${this.apiUrl}/history`);
  }

  getAnalysis(id: string): Observable<AnalysisResponse> {
    return this.http.get<AnalysisResponse>(`${this.apiUrl}/analysis/${id}`);
  }
}
