using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using elefanti.video.backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/reactions/likes")]
public class LikeController : ControllerBase {
    private readonly DbConnection _dbConnection;
    private readonly IValidator<LikePost> _validator;
    private readonly TokenService _tokenService;

    public LikeController(DbConnection dbConnection, IValidator<LikePost> validator,
                             TokenService tokenService) {
        _dbConnection = dbConnection;
        _validator = validator;
        _tokenService = tokenService;
    }

    [HttpGet("video/{videoid}")]
    public ActionResult<List<Like>> Get(string videoid) {
        var likes = _dbConnection.Likes.Where(c => c.VideoId == videoid).Include(c => c.User).ToList();

        return Ok(JsonConvert.SerializeObject(likes));
    }

    [HttpDelete]
	[Route("{id}")]
    public ActionResult<Like> DeleteLike(int id) {

        var exisitingLike = _dbConnection.Likes.FirstOrDefault(l => l.Id == id);

        if (exisitingLike is null)
            return NotFound("Cannot delete non-existing like!");

        _dbConnection.Likes.Remove(exisitingLike);
        _dbConnection.SaveChanges();

        return NoContent();
    }

}