using System;
using TaskManager.API.Models;

namespace TaskManager.API.DTOs.Tasks
{
    public class TaskFilterDto
    {
        public string? Search { get; set; }
        public Priority? Priority { get; set; }
        public TaskStatus? Status { get; set; }
        public DateTime? DueBefore { get; set; }
        public DateTime? DueAfter { get; set; }
        public string SortBy { get; set; } = "createdAt";
        public bool SortDescending { get; set; } = true;
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
