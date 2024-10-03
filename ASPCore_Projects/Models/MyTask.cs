using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ASPCore_Projects.Models
{
    public class MyTask
    {
        public int Id { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public string? UserId { get; set; }
        public IdentityUser? User { get; set; }
    }
}
