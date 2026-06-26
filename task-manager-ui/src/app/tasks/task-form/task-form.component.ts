import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskService } from '../task.service';
import { Priority, TaskStatus } from '../../models/task.model';

@Component({
  selector: 'app-task-form',
  standalone: false,
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.css']
})
export class TaskFormComponent implements OnInit {
  form!: FormGroup;
  taskId?: string;
  isEditMode = false;
  loading = false;
  errorMessage = '';

  priorities = ['Low', 'Medium', 'High'];
  statuses = ['Todo', 'InProgress', 'Done'];

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.taskId = id;
      this.isEditMode = true;
    }
    
    this.buildForm();
    
    if (this.isEditMode) {
      this.loadTask();
    }
  }

  buildForm() {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      description: [''],
      priority: ['Medium', Validators.required],
      status: ['Todo'], // Only relevant in edit mode, default to Todo
      dueDate: [null]
    });
  }

  loadTask() {
    this.loading = true;
    this.taskService.getTask(this.taskId!).subscribe({
      next: task => {
        let dateStr = null;
        if (task.dueDate) {
          dateStr = task.dueDate.split('T')[0];
        }

        this.form.patchValue({
          title: task.title,
          description: task.description,
          priority: task.priority,
          status: task.status,
          dueDate: dateStr
        });
        this.loading = false;
      },
      error: err => {
        this.errorMessage = 'Failed to load task details.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSubmit() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.value;
    
    // Map string values back to API enums
    let priorityNum = Priority.Medium;
    if (formValue.priority === 'Low') {
      priorityNum = Priority.Low;
    } else if (formValue.priority === 'High') {
      priorityNum = Priority.High;
    }

    const payload: any = {
      title: formValue.title,
      description: formValue.description,
      priority: priorityNum,
      dueDate: formValue.dueDate ? new Date(formValue.dueDate).toISOString() : null
    };

    if (this.isEditMode) {
      let statusNum = TaskStatus.Todo;
      if (formValue.status === 'InProgress') {
        statusNum = TaskStatus.InProgress;
      } else if (formValue.status === 'Done') {
        statusNum = TaskStatus.Done;
      }
      payload.status = statusNum;
    }

    this.loading = true;
    this.errorMessage = '';

    const action$ = this.isEditMode
      ? this.taskService.updateTask(this.taskId!, payload)
      : this.taskService.createTask(payload);

    action$.subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/tasks']);
      },
      error: err => {
        this.loading = false;
        this.errorMessage = err.error?.message || 'Failed to save task.';
        console.error(err);
      }
    });
  }
}
