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
[Route("api/reactions/comments")]
public class CommentController : ControllerBase {
    private readonly DbConnection _dbConnection;
    private readonly IValidator<CommentPost> _validator;
    private readonly TokenService _tokenService;

    public CommentController(DbConnection dbConnection, IValidator<CommentPost> validator,
                             TokenService tokenService) {
        _dbConnection = dbConnection;
        _validator = validator;
        _tokenService = tokenService;
    }

    [HttpGet("video/{videoid}")]
    public ActionResult<List<Comment>> Get(string videoid) {
        var comments = _dbConnection.Comments.Where(c => c.VideoId == videoid).Include(c => c.User).ToList();

        return Ok(JsonConvert.SerializeObject(comments));
    }

    [HttpGet("user/")]
    [Authorize]
    public ActionResult<List<Comment>> GetUserComments() {
        var authHeader = HttpContext.Request.Headers.Authorization.ToString();
        
        var tokenPayload = _tokenService.GetTokenPayload(authHeader);
        var userIdIsValid = int.TryParse(tokenPayload["Id"].ToString(), out var userid);

        if (!userIdIsValid)
            return BadRequest("Invalid user id");

        var userComments = _dbConnection.Comments.Where(c => c.UserId == userid).ToList();

        return Ok(JsonConvert.SerializeObject(userComments));
    }
}

