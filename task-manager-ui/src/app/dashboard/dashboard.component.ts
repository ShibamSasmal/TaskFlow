import { Component, OnInit } from '@angular/core';
import { TaskService } from '../tasks/task.service';
import { DashboardStats, Task, TaskStatus } from '../models/task.model';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats | null = null;
  loading = true;
  error = '';

  constructor(private taskService: TaskService) {}

  ngOnInit() {
    this.loadStats();
  }

  loadStats() {
    this.loading = true;
    this.error = '';
    this.taskService.getDashboardStats().subscribe({
      next: data => {
        this.stats = data;
        this.loading = false;
      },
      error: err => {
        this.error = 'Failed to load dashboard statistics.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onStatusChange(task: Task, newStatusStr: string) {
    let newStatusNum = TaskStatus.Todo;
    if (newStatusStr === 'InProgress') {
      newStatusNum = TaskStatus.InProgress;
    } else if (newStatusStr === 'Done') {
      newStatusNum = TaskStatus.Done;
    }

    this.taskService.updateStatus(task.id, newStatusNum).subscribe({
      next: () => {
        this.loadStats();
      },
      error: err => {
        console.error('Failed to update status', err);
      }
    });
  }
}
