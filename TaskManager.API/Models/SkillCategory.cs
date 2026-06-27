using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.Models
{
    public class SkillCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Skill> Skills { get; set; } = new List<Skill>();
    }
}
