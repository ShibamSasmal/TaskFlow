using System;
using System.ComponentModel.DataAnnotations;
using TaskManager.API.Models;

namespace TaskManager.API.DTOs.Tasks
{
    public class CreateTaskDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0, 2)]
        public Priority Priority { get; set; } = Priority.Medium;

        public DateTime? DueDate { get; set; }
    }
}
