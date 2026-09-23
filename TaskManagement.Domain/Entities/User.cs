using System.ComponentModel.DataAnnotations;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Team> ManagedTeams { get; set; } = new List<Team>();

        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

        public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();

        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}