using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using elefanti.video.backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/reactions/likes")]
public class LikeController : ControllerBase {
    private readonly DbConnection _dbConnection;
    private readonly TokenService _tokenService;

    public LikeController(DbConnection dbConnection, TokenService tokenService) {
        _dbConnection = dbConnection;
        _tokenService = tokenService;
    }

    [HttpGet("{videoid}")]
    public ActionResult<List<Like>> Get(string videoid) {
        var likes = _dbConnection.Likes.Where(c => c.VideoId == videoid)
            .Select(l => new {
                l.Id,
                l.VideoId,
                l.UserId,
                User = new {
                    l.User.Username,
                    l.User.Id
                }
            }).ToList();

        return Ok(JsonConvert.SerializeObject(likes));
    }

    [HttpGet]
    [Authorize]
    [Route("user")]
    public ActionResult<List<Like>> GetUserLikes() {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var tokenPayload = _tokenService.GetTokenPayload(authHeader);

        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);
        if (!userIdIsValid)
            return BadRequest("Invalid user id");

        var allLikes = _dbConnection.Likes.Where(l => l.UserId == userid).ToList();

        return Ok(JsonConvert.SerializeObject(allLikes));
    }

    [HttpPost]
    [Authorize]
    [Route("{videoid}")]
    public ActionResult<Like> AddLike(string videoid) {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var tokenPayload = _tokenService.GetTokenPayload(authHeader);

        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);
        if (!userIdIsValid)
            return BadRequest("Invalid user id");


        var video = _dbConnection.Videos.FirstOrDefault(v => v.Id == videoid);
        if (video is null)
            return NotFound("Video was not found");

        var existingLike = _dbConnection.Likes.FirstOrDefault(l => l.VideoId == videoid && l.UserId == userid);
        if (existingLike is not null)
            return Conflict("You have already liked this video");

        var addedLike = _dbConnection.Likes.Add(
            new() {
                VideoId = videoid,
                UserId = userid
            }
         );

        _dbConnection.SaveChanges();
        return CreatedAtAction(nameof(Get), new { id = addedLike.Entity.Id }, addedLike.Entity);
    }


    [HttpDelete]
    [Authorize]
    [Route("{videoid}")]
    public ActionResult<Like> RemoveLike(string videoid) {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();

        var tokenPayload = _tokenService.GetTokenPayload(authHeader);
        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);

        if (!userIdIsValid)
            return BadRequest("Invalid user id");

        var exisitingLike = _dbConnection.Likes.FirstOrDefault(l => l.VideoId == videoid);

        if (exisitingLike is null)
            return NotFound("Like is not found!");


        if (exisitingLike.UserId != userid)
            return Unauthorized();


        _dbConnection.Likes.Remove(exisitingLike);
        _dbConnection.SaveChanges();

        return NoContent();
    }

}