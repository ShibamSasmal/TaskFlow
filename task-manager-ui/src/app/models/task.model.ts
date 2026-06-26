export enum Priority {
  Low = 0,
  Medium = 1,
  High = 2
}

export enum TaskStatus {
  Todo = 0,
  InProgress = 1,
  Done = 2
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  priority: string; // "Low" | "Medium" | "High"
  status: string; // "Todo" | "InProgress" | "Done"
  dueDate?: string;
  isOverdue: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface TaskFilter {
  search?: string;
  priority?: number;
  status?: number;
  dueBefore?: string;
  dueAfter?: string;
  sortBy: string;
  sortDescending: boolean;
  page: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface DashboardStats {
  totalTasks: number;
  todoCount: number;
  inProgressCount: number;
  doneCount: number;
  overdueCount: number;
  highPriorityCount: number;
  completionRate: number;
  upcomingTasks: Task[];
}
