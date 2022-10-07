using System.ComponentModel.DataAnnotations;

namespace elefanti.video.backend.Models;
public class Category {
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}
