using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using elefanti.video.backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/users")]
public class AuthenticationController : ControllerBase {
    public readonly DbConnection _dbConnection;
    public readonly TokenService _tokenService;

    public AuthenticationController(DbConnection dbConnection, TokenService tokenService) {
        _dbConnection = dbConnection;
        _tokenService = tokenService;
    }

    [HttpPost]
    [Route("login")]
    public ActionResult<TokenResponse> Login([FromBody] UserCredentials userCredentials) {

        if (userCredentials is null)
            return BadRequest("Invalid User credentials");

        var user = _dbConnection.Users.FirstOrDefault(user => user.Username == userCredentials.Username);

        if (user is null)
            return NotFound();

        if (userCredentials.Password != user.Password)
            return Unauthorized();

        var tokenResponse = _tokenService.GenerateToken(user);

        return Ok(tokenResponse);
    }

    [HttpPost]
    [Route("signup")]
    public ActionResult<TokenResponse> Signup([FromBody] User user) {
        if (user is null)
            return BadRequest("Invalid Input");

        // validate user input, and hash password

        var userNameExists = _dbConnection.Users.Any(c => c.Username == user.Username);

        if (userNameExists)
            return Conflict("Username already exists");

        var userEntity = _dbConnection.Users.Add(user);
        _dbConnection.SaveChanges();
        user.Id = userEntity.CurrentValues.GetValue<int>("Id");

        var tokenResponse = _tokenService.GenerateToken(user);

        return Ok(tokenResponse);
    }
}

