using Microsoft.AspNetCore.Identity;
namespace Infrastructure.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
