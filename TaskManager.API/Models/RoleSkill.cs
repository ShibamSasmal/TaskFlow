using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.API.Models
{
    public class RoleSkill
    {
        public int RoleTemplateId { get; set; }

        [ForeignKey(nameof(RoleTemplateId))]
        public RoleTemplate RoleTemplate { get; set; } = null!;

        public int SkillId { get; set; }

        [ForeignKey(nameof(SkillId))]
        public Skill Skill { get; set; } = null!;

        public bool IsRequired { get; set; } = true; // true = required, false = preferred
    }
}
