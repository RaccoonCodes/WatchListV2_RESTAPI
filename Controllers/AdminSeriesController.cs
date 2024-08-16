using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json;
using WatchListV2.DTO;
using WatchListV2.Models;
using System.Linq.Dynamic.Core;
using WatchListV2.Extensions;
using Microsoft.AspNetCore.Authorization;
using WatchListV2.Constants;

namespace WatchListV2.Controllers
{
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = RoleNames.Administrator)]

    public class AdminSeriesController : ControllerBase
    {
       private readonly IAdminSeriesService _adminSeriesService;
        public AdminSeriesController(IAdminSeriesService adminSeriesService)
            => _adminSeriesService = adminSeriesService;

        //GET
        [HttpGet("all-users-series", Name = "GetAllUsersSeries")]
        [ResponseCache(CacheProfileName = "Any-60")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets all users and their series",
            Description = "Retrieves a list of all info on each users with pagination and filters. " +
            "Accessible only by administrators."
        )]
        public async Task<ActionResult<RestDTO<SeriesModel[]>>> GetAdminSeries([FromQuery] RequestDTO<SeriesDTO> input)
        {
            var result = await _adminSeriesService.GetAllSeriesUserAsync(input);
            result.Links.Add(new LinkDTO( Url.Action(null, "AdminSeries",
                   new { input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET"));

            if (result.Data.Length == 0)
            {
               
                return NotFound(result);
            }

            return Ok(result);
        }
        //DELETE
        [HttpDelete("delete-user/{userId}", Name = "DeleteUserAndSeries")]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Deletes a user and their series",
            Description = "Deletes selected user and their associated series from the database. " +
            "Accessible only by administrators."
        )]
        public async Task<ActionResult> DeleteUserAndSeries(
             [SwaggerParameter("The ID of the user to be deleted")] string userId)
        {
            var sucessResults = await _adminSeriesService.DeleteUserAndSeriesAsync(userId);

            if (!sucessResults)
            {
                return NotFound("User not found or failed to delete user");
            }

            return Ok("Sucessfully Deleted");

        }

    }
}
