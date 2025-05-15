namespace Core.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string UserName { get; set; }
        public string Email { get; set; }
        ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}   
