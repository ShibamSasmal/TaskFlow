import { Component, OnInit } from '@angular/core';
import { TaskService } from '../task.service';
import { Task, TaskFilter, TaskStatus } from '../../models/task.model';

@Component({
  selector: 'app-task-list',
  standalone: false,
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  filter: TaskFilter = {
    search: '',
    priority: undefined,
    status: undefined,
    dueBefore: undefined,
    dueAfter: undefined,
    sortBy: 'createdAt',
    sortDescending: true,
    page: 1,
    pageSize: 5
  };
  totalCount = 0;
  totalPages = 1;
  loading = true;
  error = '';

  constructor(private taskService: TaskService) {}

  ngOnInit() {
    this.loadTasks();
  }

  loadTasks() {
    this.loading = true;
    this.error = '';
    
    // Normalize filter fields (empty strings to undefined)
    const queryFilter = { ...this.filter };
    if (!queryFilter.search) {
      queryFilter.search = undefined;
    }
    if (queryFilter.dueBefore === '') {
      queryFilter.dueBefore = undefined;
    }
    if (queryFilter.dueAfter === '') {
      queryFilter.dueAfter = undefined;
    }

    this.taskService.getTasks(queryFilter).subscribe({
      next: result => {
        this.tasks = result.items;
        this.totalCount = result.totalCount;
        this.totalPages = result.totalPages || 1;
        this.loading = false;
      },
      error: err => {
        this.error = 'Failed to load tasks. Please try again.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onFilterChange() {
    this.filter.page = 1;
    this.loadTasks();
  }

  resetFilters() {
    this.filter = {
      search: '',
      priority: undefined,
      status: undefined,
      dueBefore: undefined,
      dueAfter: undefined,
      sortBy: 'createdAt',
      sortDescending: true,
      page: 1,
      pageSize: 5
    };
    this.loadTasks();
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
        this.loadTasks();
      },
      error: err => {
        console.error('Failed to update status', err);
      }
    });
  }

  onPageChange(page: number) {
    if (page >= 1 && page <= this.totalPages) {
      this.filter.page = page;
      this.loadTasks();
    }
  }
}
