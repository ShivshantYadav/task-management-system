using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Domain.Entities
{
    public class TeamMember
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int TeamId { get; set; }

        [Range(1, int.MaxValue)]
        public int UserId { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public Team Team { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}