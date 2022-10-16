using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace elefanti.video.backend.Models;
public class Category {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string ImageName { get; set; }
}

public class CategoryDto { 
    public string Name { get; set; }
    public IFormFile ImageFile { get; set; }
}

public class CategoryValidator : AbstractValidator<CategoryDto> {
    public CategoryValidator() {
        RuleFor(c => c.Name).NotNull().NotEmpty();
        RuleFor(c => c.ImageFile).NotNull().NotEmpty();
        RuleFor(c => c.ImageFile.ContentType).Matches("jpg|png|jpeg|webp").WithMessage("File uploaded is not a jpg, png, jpeg, or webp");
    }
}