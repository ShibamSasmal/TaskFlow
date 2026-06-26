using System;
using System.ComponentModel.DataAnnotations;
using TaskManager.API.Models;

namespace TaskManager.API.DTOs.Tasks
{
    public class UpdateTaskDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Priority? Priority { get; set; }
        public TaskStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
