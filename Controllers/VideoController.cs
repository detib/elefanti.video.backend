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

	[HttpGet("search/{query}")]
	public ActionResult<List<Video>> SearchVideos(string query) {
		var videos = _dbConnection.Videos
						.Include(v => v.Category)
						.Where(v => v.Title.ToLower().Contains(query.ToLower()))
						.ToList();
		return Ok(videos);
	} 

	[HttpGet]
	public ActionResult<Video> Get(int from, int take) {
		if (take < 1)
			return BadRequest("You cannot take less than 1 videos");

		var allVideos = _dbConnection.Videos
							.Skip(from)
							.Take(take)
							.OrderByDescending(v => v.CreatedOn)
							.ToList();
		return Ok(allVideos);
	}

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

	[HttpGet("category/{categoryid}")]
	public ActionResult<Video> GetByCategory(int categoryid) {

		var videosInCategory = _dbConnection.Videos
									.Where(v => v.CategoryId == categoryid)
									.OrderBy(v => v.Views)
									.Include(v => v.Category)
									.ToList();
		return Ok(videosInCategory);
	}

	[HttpPost]
	[Authorize(Roles = "admin")]
	public ActionResult<Video> PostVideo([FromBody] VideoPost video) {

		var categoryExists = _dbConnection.Categories.Any(c => c.Id == video.CategoryId);

		if (!categoryExists)
			return Conflict("Category does not exist");

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
				TimeStamp = video.TimeStamp,
				CreatedOn = DateTime.UtcNow
			}
			);
		_dbConnection.SaveChanges();

		return CreatedAtAction(nameof(Get), new { id = videoAdded.Entity.Id }, video);
    }

    [HttpPut]
    [Route("{id}")]
    [Authorize(Roles = "admin")]
    public ActionResult<Video> UpdateVideo(string id,[FromBody] VideoPut video) {

        var existingVideo = _dbConnection.Videos.FirstOrDefault(v => v.Id == id);

        if (existingVideo is null)
            return NotFound("Category does not exist");

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

