import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Task } from '../../models/task.model';
import { TaskService } from '../task.service';

@Component({
  selector: 'app-task-card',
  standalone: false,
  templateUrl: './task-card.component.html',
  styleUrls: ['./task-card.component.css']
})
export class TaskCardComponent {
  @Input() task!: Task;
  @Output() statusChange = new EventEmitter<string>();
  @Output() delete = new EventEmitter<void>();
  deleting = false;

  constructor(private taskService: TaskService) {}

  onStart() {
    this.statusChange.emit('InProgress');
  }

  onComplete() {
    this.statusChange.emit('Done');
  }

  onReopen() {
    this.statusChange.emit('Todo');
  }

  onDelete() {
    if (confirm(`Are you sure you want to delete "${this.task.title}"?`)) {
      this.deleting = true;
      this.taskService.deleteTask(this.task.id).subscribe({
        next: () => {
          this.deleting = false;
          this.delete.emit();
        },
        error: err => {
          this.deleting = false;
          console.error(err);
          alert('Failed to delete task.');
        }
      });
    }
  }
}
