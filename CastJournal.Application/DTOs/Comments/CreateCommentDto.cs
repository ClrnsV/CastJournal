using System.ComponentModel.DataAnnotations;

namespace CastJournal.Application.DTOs.Comments;
public class CreateCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;
}