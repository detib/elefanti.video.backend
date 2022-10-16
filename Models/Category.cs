using FluentValidation;
using System.ComponentModel.DataAnnotations;

namespace elefanti.video.backend.Models;
public class Category {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}

public class CategoryDto { 
    public string Name { get; set; }
}

public class CategoryValidator : AbstractValidator<CategoryDto> {
    public CategoryValidator() {
        RuleFor(c => c.Name).NotNull().NotEmpty();
    }
}