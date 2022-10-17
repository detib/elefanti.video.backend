using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/videos")]
public class VideoController : ControllerBase {
    private readonly DbConnection _dbConnection;
    private readonly IValidator<VideoPost> _validator;

    public VideoController(DbConnection dbConnection, IValidator<VideoPost> validator) {
        _dbConnection = dbConnection;
        _validator = validator;
    }

    /**
     * This method recieves a search query.
     * Returns a list of videos that refer to the query.
     **/ 

    [HttpGet("search/{query}")]
    public ActionResult<List<Video>> SearchVideos(string query) {
        var videos = _dbConnection.Videos
        				.Include(v => v.Category)
        				.Where(v => v.Title.ToLower().Contains(query.ToLower()))
        				.ToList();        
        return Ok(videos);
    }

    /**
     * This method recieves parameters, starting position and amount of videos to be returned.
     * Returns Bad request if amount of videos to return is null or less than 1.
     * Returns list of videos in that range.
     **/ 

    [HttpGet]
    public ActionResult<Video> Get(int from, int take) {
        if (take < 1)
            return BadRequest("You cannot take less than 1 videos");

        var allVideos = _dbConnection.Videos
                            .OrderByDescending(v => v.CreatedOn)
                            .Skip(from)
                            .Take(take)
                            .ToList();
        return Ok(allVideos);
    }

    /**
     * This method recieves a videoId.
     * VideoId parameter cannot be null.
     * Returns the specific video with that Id in the case that it exists.
     **/

    [HttpGet("{id}")]
    public ActionResult<Video> GetVideo(string id) {

        var video = _dbConnection.Videos.Include(v => v.Category).FirstOrDefault(v => v.Id == id);

        if (video is null)
            return NotFound("Video does not exist");

        video.Views++;
        _dbConnection.Videos.Update(video);
        _dbConnection.SaveChanges();

        return Ok(video);
    }

    /**
     * This method recieves a categoryId.
     * CategoryId parameter must not be null.
     * Returns the category with that Id in the case that it exists.
     **/

    [HttpGet("category/{categoryid}")]
    public ActionResult<Video> GetByCategory(int categoryid) {

        var videosInCategory = _dbConnection.Videos
                                    .Where(v => v.CategoryId == categoryid)
                                    .OrderBy(v => v.Views)
                                    .Include(v => v.Category)
                                    .ToList();
        return Ok(videosInCategory);
    }

    /**
     * This method is available only to Admin users to create new posts.
     * Returns Bad request in failure during Validation, non-existent Category.
     * Returns Conflict in the case that Video already exists.
     * Returns OK and adds Video to Database.
     **/
    [HttpPost]
    [Authorize(Roles = "admin")]
    public ActionResult<Video> PostVideo([FromBody] VideoPost video) {

        var categoryExists = _dbConnection.Categories.Any(c => c.Id == video.CategoryId);

        if (!categoryExists)
            return BadRequest("Category does not exist");

        var validationResult = _validator.Validate(video);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);

        var existingVideo = _dbConnection.Videos.FirstOrDefault(v => v.Id == video.Id);
        if (existingVideo is not null)
            return Conflict("Video already exists");


        var videoAdded = _dbConnection.Videos.Add(
            new() {
                Id = video.Id,
                Title = video.Title,
                Description = video.Description,
                CategoryId = video.CategoryId,
                CreatedOn = DateTime.UtcNow
            }
            );
        _dbConnection.SaveChanges();

        return CreatedAtAction(nameof(Get), new { id = videoAdded.Entity.Id }, video);
    }

    /**
     * This method is only available to Admin users to update existing videos.
     * Returns Not Found if video with recieved VideoId does not exist.
     * Returns Ok and updates existing Video values.
     **/ 

    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Video> UpdateVideo(string id, [FromBody] VideoPut video) {

        var existingVideo = _dbConnection.Videos.FirstOrDefault(v => v.Id == id);

        if (existingVideo is null)
            return NotFound("Video does not exist");

        if (video.CategoryId is not null)
            existingVideo.CategoryId = (int)video.CategoryId;
        if (video.Title is not null)
            existingVideo.Title = video.Title;
        if (video.Description is not null)
            existingVideo.Description = video.Description;

        _dbConnection.Videos.Update(existingVideo);
        _dbConnection.SaveChanges();

        return Ok(existingVideo);
    }

    /**
     * This method is only available to Admin users to delete existing videos.
     * Returns Not Found if video with recieved VideoId does not exist.
     * Returns Ok and removes Video from the database.
     **/ 

    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Category> DeleteVideo(string id) {

        var existingVideo = _dbConnection.Videos.FirstOrDefault(v => v.Id == id);

        if (existingVideo is null)
            return NotFound("Video does not exist");

        _dbConnection.Videos.Remove(existingVideo);
        _dbConnection.SaveChanges();

        return NoContent();
    }
}

