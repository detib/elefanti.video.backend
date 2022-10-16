using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elefanti.video.backend.Models;
public class User {

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public Role Role { get; set; }

}

public class UserPost {
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public enum Role {
    admin,
    user
}

public class UserValidator : AbstractValidator<UserPost> {
    public UserValidator() {
        RuleFor(user => user.Name).NotNull().NotEmpty();
        RuleFor(user => user.Surname).NotNull().NotEmpty();
        RuleFor(user => user.Username).NotNull().NotEmpty();
        RuleFor(user => user.Password).NotNull().NotEmpty().MinimumLength(8);
    }
}