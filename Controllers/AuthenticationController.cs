using elefanti.video.backend.Data;
using elefanti.video.backend.Models;
using elefanti.video.backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace elefanti.video.backend.Controllers;

[ApiController]
[Route("api/users")]
public class AuthenticationController : ControllerBase {
    public readonly DbConnection _dbConnection;
    public readonly TokenService _tokenService;
    public readonly PasswordService _passwordService;
    private readonly IValidator<UserPost> _userValidator;

    public AuthenticationController(DbConnection dbConnection, TokenService tokenService, 
                                    PasswordService passwordService, IValidator<UserPost> uservalidator) {
        _dbConnection = dbConnection;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _userValidator = uservalidator;
    }

    /**
     * This method validates User credentials during login.
     * Returns Bad Request in the case of invalid credentials.
     * Returns Not Found if User does not exist.
     * Returns Unauthorized in the case of invalid Password.
     * Returns Ok in the case of valid credentials.
     **/

    [HttpPost]
    [Route("login")]
    public ActionResult<TokenResponse> Login([FromBody] UserCredentials userCredentials) {

        if (userCredentials is null)
            return BadRequest("Invalid User credentials");

        var user = _dbConnection.Users.FirstOrDefault(user => user.Username == userCredentials.Username);

        if (user is null)
            return NotFound();

        if (!_passwordService.VerifyPassword(userCredentials.Password, user.Password))
            return Unauthorized();

        var tokenResponse = _tokenService.GenerateToken(user);

        return Ok(tokenResponse);
    }

    /**
     * This method validates User credentials during signup.
     * Returns Bad Request in the case of invalid credential input, failure in User validation.
     * Returns Conflict in the case of existing username.
     * Returns Ok, creates and adds User to the Database.
     **/

    [HttpPost]
    [Route("signup")]
    public ActionResult Signup([FromBody] UserPost user) {
        if (user is null)
            return BadRequest("Invalid Input");


        var validationResult = _userValidator.Validate(user);
        if(!validationResult.IsValid) 
            return BadRequest(validationResult.Errors);

        var userNameExists = _dbConnection.Users.Any(c => c.Username == user.Username);
        if (userNameExists)
            return Conflict("Username already exists");

        user.Password = _passwordService.HashPassword(user.Password);

        var newUser = new User() {
            Name = user.Username,
            Surname = user.Surname,
            Username = user.Username,
            Password = user.Password,
            Role = Role.user
        };

        var userEntity = _dbConnection.Users.Add(newUser);
        _dbConnection.SaveChanges();

        return Ok();
    }
}

