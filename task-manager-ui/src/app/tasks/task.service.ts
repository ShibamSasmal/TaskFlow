import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Task, TaskFilter, PagedResult, DashboardStats } from '../models/task.model';

@Injectable({ providedIn: 'root' })
export class TaskService {
  private apiUrl = `${environment.apiUrl}/tasks`;

  constructor(private http: HttpClient) {}

  getTasks(filter: TaskFilter): Observable<PagedResult<Task>> {
    let params = new HttpParams();
    
    if (filter.search) {
      params = params.set('search', filter.search);
    }
    if (filter.priority !== undefined && filter.priority !== null && filter.priority.toString() !== '') {
      params = params.set('priority', filter.priority.toString());
    }
    if (filter.status !== undefined && filter.status !== null && filter.status.toString() !== '') {
      params = params.set('status', filter.status.toString());
    }
    if (filter.dueBefore) {
      params = params.set('dueBefore', filter.dueBefore);
    }
    if (filter.dueAfter) {
      params = params.set('dueAfter', filter.dueAfter);
    }
    if (filter.sortBy) {
      params = params.set('sortBy', filter.sortBy);
    }
    if (filter.sortDescending !== undefined) {
      params = params.set('sortDescending', filter.sortDescending.toString());
    }
    if (filter.page) {
      params = params.set('page', filter.page.toString());
    }
    if (filter.pageSize) {
      params = params.set('pageSize', filter.pageSize.toString());
    }

    return this.http.get<PagedResult<Task>>(this.apiUrl, { params });
  }

  getTask(id: string): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/${id}`);
  }

  createTask(dto: any): Observable<Task> {
    return this.http.post<Task>(this.apiUrl, dto);
  }

  updateTask(id: string, dto: any): Observable<Task> {
    return this.http.put<Task>(`${this.apiUrl}/${id}`, dto);
  }

  updateStatus(id: string, status: number): Observable<void> {
    // Pass status as JSON body (integer)
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, status, {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }
}
