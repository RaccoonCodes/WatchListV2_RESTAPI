using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WatchListV2.Models;
using Swashbuckle.AspNetCore.Annotations;
using WatchListV2.DTO;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace WatchListV2.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
       private readonly IUserService _userService;

        public AccountController(IUserService userService) => 
            _userService = userService;

        [HttpPost]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(Summary = "Registers New Users", Description = "A DTO containing user data")]
        public async Task <ActionResult> Register(RegisterDTO input)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var result = await _userService.RegisterAsync(input);
                    if (result.Succeeded)
                    {
                        return StatusCode(201);
                    }
                    else
                    {
                        throw new Exception(string.Format("Error {0}", string.Join(" ",
                             result.Errors.Select(e => e.Description))));
                    }
                }
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            catch (Exception ex) {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status = StatusCodes.Status500InternalServerError;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";

                return StatusCode(StatusCodes.Status500InternalServerError);

            }
        }

        [HttpPost]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Performs a user login", Description = "A DTO containing user's credentials")]
        public async Task <ActionResult> Login(LoginDTO input)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var jwtString = await _userService.LoginAsync(input);
                    return StatusCode(StatusCodes.Status200OK, jwtString);
                }
                //invalid input
                else
                {
                    var details = new ValidationProblemDetails(ModelState);
                    details.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                    details.Status = StatusCodes.Status400BadRequest;
                    return new BadRequestObjectResult(details);
                }
            }
            //if user doesn't exist, password doesn't match, and/or exceptions occurs
            catch (Exception ex) {
                var exceptionDetails = new ProblemDetails();
                exceptionDetails.Detail = ex.Message;
                exceptionDetails.Status = StatusCodes.Status401Unauthorized;
                exceptionDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                return StatusCode(StatusCodes.Status401Unauthorized, exceptionDetails);
            }
        }

       
    }
}