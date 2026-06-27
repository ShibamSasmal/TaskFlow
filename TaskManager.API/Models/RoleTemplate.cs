using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManager.API.Models
{
    public class RoleTemplate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<RoleSkill> RoleSkills { get; set; } = new List<RoleSkill>();
    }
}
