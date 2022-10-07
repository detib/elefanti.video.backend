﻿using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace elefanti.video.backend.Models;

public class Video {

    [Key]
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }
    public DateTime TimeStamp { get; set; }
    public DateTime CreatedOn { get; set; }
    public int Views { get; set; }
}

public class VideoPost {
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int CategoryId { get; set; }
    public DateTime TimeStamp { get; set; }
}

public class VideoPut {
    public string Title { get; set; }
    public string Description { get; set; }
    public int? CategoryId { get; set; }
}


public class VideoValidator : AbstractValidator<VideoPost> {
    public VideoValidator() {
        RuleFor(video => video.Id).NotNull().NotEmpty();
        RuleFor(video => video.Title).NotNull().NotEmpty();
        RuleFor(video => video.CategoryId).NotNull().NotEmpty();
        RuleFor(video => video.TimeStamp).NotNull().NotEmpty();
    }
}