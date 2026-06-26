using System;

namespace TaskManager.API.Models
{
    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum TaskStatus
    {
        Todo = 0,
        InProgress = 1,
        Done = 2
    }

    public class TaskItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public TaskStatus Status { get; set; } = TaskStatus.Todo;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key & Navigation
        public Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
    }
}
