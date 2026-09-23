using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Domain.Entities
{
    public class Team
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ManagerId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User Manager { get; set; } = null!;

        public ICollection<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}