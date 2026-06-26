using System;
using System.Collections.Generic;

namespace TaskManager.API.Models
{
    public class AppUser
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
