using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase {
    private readonly DbConnection _dbConnection;

    public CategoryController(DbConnection dbConnection) {
        _dbConnection = dbConnection;
    }

    [HttpGet]
    public ActionResult<List<Category>> Get() {
        var categories = JsonConvert.SerializeObject(_dbConnection.Categories.ToList());
        return Ok(categories);
    }

    [HttpGet("{id}")] // ? 
    public ActionResult<Category> GetCategory(int id) {
        var category = _dbConnection.Categories.FirstOrDefault(c => c.Id == id);
        if (category is null)
            return NotFound("Category does not exist");
        return Ok(category);
    }


    [HttpPost]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> PostCategory(Category category) {

        var existingCategory = _dbConnection.Categories.FirstOrDefault(c => c.Name.ToLower() == category.Name.ToLower());

        if (existingCategory != null)
            return Conflict("Category already Exists");

        var newCategory = _dbConnection.Categories.Add(category);
        _dbConnection.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = newCategory.Entity.Id}, category);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> UpdateCategory(int id, Category category) {

        var existingCategory = _dbConnection.Categories.FirstOrDefault(c => c.Id == id);
        
        if (existingCategory is null)
            return NotFound("Category does not exist");

        existingCategory.Name = category.Name;
        _dbConnection.Categories.Update(existingCategory);
        _dbConnection.SaveChanges();

        return Ok(existingCategory);
    }

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
