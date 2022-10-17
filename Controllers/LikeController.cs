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

    /**
     * This method returns amount of likes in a video.
     **/ 

    [HttpGet("{videoid}")]
    public async Task<ActionResult<List<Like>>> Get(string videoid) {
        var likes = await _dbConnection.Likes.Where(c => c.VideoId == videoid)
            .Select(l => new {
                l.Id,
                l.VideoId,
                l.UserId,
                User = new {
                    l.User.Username,
                    l.User.Id
                }
            }).ToListAsync();

        return Ok(JsonConvert.SerializeObject(likes));
    }

    /**
     * This method returns list of User likes.
     * Returns Bad Request if User Id is invalid.
     **/ 

    [HttpGet]
    [Authorize]
    [Route("user")]
    public async Task<ActionResult<List<Like>>> GetUserLikes() {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var tokenPayload = _tokenService.GetTokenPayload(authHeader);

        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);
        if (!userIdIsValid)
            return BadRequest("Invalid user id");

        var allLikes = await _dbConnection.Likes.Where(l => l.UserId == userid).Include(l => l.Video).ToListAsync();

        return Ok(JsonConvert.SerializeObject(allLikes));
    }

    /**
     * This method handles adding likes to videos using VideoId.
     **/ 

    [HttpPost]
    [Authorize]
    [Route("{videoid}")]
    public async Task<ActionResult<Like>> AddLike(string videoid) {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        var tokenPayload = _tokenService.GetTokenPayload(authHeader);

        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);
        if (!userIdIsValid)
            return BadRequest("Invalid user id");


        var existingLike = await _dbConnection.Likes.FirstOrDefaultAsync(l => l.VideoId == videoid && l.UserId == userid);
        if (existingLike is not null)
            return Conflict("You have already liked this video");

        var addedLike = await _dbConnection.Likes.AddAsync(
            new() {
                VideoId = videoid,
                UserId = userid
            }
         );

        await _dbConnection.SaveChangesAsync();
        // return CreatedAtAction(nameof(Get), new { id = addedLike.Entity.Id }, addedLike.Entity);
        return Ok(addedLike.Entity);
    }

    /**
     * This method handles removing likes from videos using VideoId.
     **/ 

    [HttpDelete]
    [Authorize]
    [Route("{videoid}")]
    public async Task<ActionResult<Like>> RemoveLike(string videoid) {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();

        var tokenPayload = _tokenService.GetTokenPayload(authHeader);
        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);

        if (!userIdIsValid)
            return BadRequest("Invalid user id");

        var exisitingLike = await _dbConnection.Likes.FirstOrDefaultAsync(l => l.VideoId == videoid);

        if (exisitingLike is null)
            return NotFound("Like is not found!");


        if (exisitingLike.UserId != userid)
            return Unauthorized();


        _dbConnection.Likes.Remove(exisitingLike);
        await _dbConnection.SaveChangesAsync();
        return NoContent();
    }

}