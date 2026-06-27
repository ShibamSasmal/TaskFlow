using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Models
{
    public class Skill
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public SkillCategory Category { get; set; } = null!;

        [MaxLength(1000)]
        public string? Aliases { get; set; } // Comma-separated alternative names for exact match, e.g. "c#,c sharp,csharp"
    }
}
