using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase {
    private readonly DbConnection _dbConnection;
    private readonly IValidator<CategoryDto> _validator;
    public CategoryController(DbConnection dbConnection, IValidator<CategoryDto> validator) {
        _dbConnection = dbConnection;
        _validator = validator;
    }

    /**
     * This method returns list of existing Categories.
     **/ 

    [HttpGet]
    public ActionResult<List<Category>> Get() {
        var categories = JsonConvert.SerializeObject(_dbConnection.Categories.ToList());
        return Ok(categories);
    }

    /**
     * This method retuns specific Category in terms with requested CategoryId.
     * Returns Not Found in the case of non-existent Category with requested CategoryId
     * Returns Ok and Category in case of success..
     **/

    [HttpGet("{id}")] // ? 
    public ActionResult<Category> GetCategory(int id) {
        var category = _dbConnection.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
            return NotFound("Category does not exist");
        return Ok(category);
    }

    /**
     * This method is available only to Admin users to create new Categories.
     * Returns Bad request in failure during Category validation.
     * Returns Conflict in the case that Category already exists.
     * Returns CreatedAtAction and adds Category to Database.
     **/

    [HttpPost]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> PostCategory([FromForm] CategoryDto category) {

        var validationResult = _validator.Validate(category);

        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var existingCategory = _dbConnection.Categories.FirstOrDefault(c => c.Name.ToLower() == category.Name.ToLower());

        if (existingCategory != null)
            return Conflict("Category already Exists");

        var newCategory = new Category() {
            Name = category.Name,
            ImageName = $"{Guid.NewGuid()}{Path.GetExtension(category.ImageFile.FileName)}"
        };

        Console.WriteLine(newCategory.ImageName);

        //save image to assets folder
        using (var fileStream = new FileStream(Path.Combine("/app", "assets", "category-images", newCategory.ImageName), FileMode.Create)) {
            category.ImageFile.CopyTo(fileStream);
        }

        var addedCategory = _dbConnection.Categories.Add(newCategory);
        _dbConnection.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = addedCategory.Entity.Id }, category);
    }

    /**
     * This method is only available to Admin users to update existing Categories.
     * Returns Not Found if Category with recieved CategoryId does not exist.
     * Returns BadRequest if invalid Category Name input.
     * Returns Ok and updates existing Category values.
     **/

    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> UpdateCategory(int id, [FromBody] CategoryUpdateDto category) {

        var existingCategory = _dbConnection.Categories.FirstOrDefault(c => c.Id == id);

        if (existingCategory is null)
            return NotFound("Category does not exist");

        if (category.Name == null || category.Name == "")
            return BadRequest("Category Name cannot be empty");

        existingCategory.Name = category.Name;
        _dbConnection.Categories.Update(existingCategory);
        _dbConnection.SaveChanges();

        return Ok(existingCategory);
    }

    /**
     * This method is only available to Admin users to delete existing Categories.
     * Returns Not Found if Category with recieved CategoryId does not exist.
     * Returns Ok and removes Category from the database.
     **/

    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> DeleteCategory(int id) {

        var existingCategory = _dbConnection.Categories.FirstOrDefault(c => c.Id == id);

        if (existingCategory is null)
            return NotFound("Category does not exist");

        _dbConnection.Categories.Remove(existingCategory); // Check again for cascade delete
        _dbConnection.SaveChanges();

        return NoContent();
    }
}
