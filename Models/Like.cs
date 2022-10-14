using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elefanti.video.backend.Models;
public class Like {
    [Key]
    public int Id { get; set; }
    public string VideoId { get; set; }
    [ForeignKey("VideoId")]
    public Video Video { get; set; }
    public int UserId { get; set; }
    [ForeignKey("UserId")]
    public User User { get; set; }

}

public class LikePost {
    public string VideoId { get; set; }
}

public class LikeValidator : AbstractValidator<LikePost> {
    public LikeValidator() {
        RuleFor(c => c.VideoId).NotNull().NotEmpty();
    }
}