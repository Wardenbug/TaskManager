using System.ComponentModel.DataAnnotations;

namespace Presentation.DTOs
{
    public class CreateTaskRequest
    {
        [Required]
        [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
        public string Title { get; set; }

        [StringLength(maximumLength: 500, ErrorMessage = "Description cannot be longer than 500 characters.")]
        public string Description { get; set; }
    }
}
