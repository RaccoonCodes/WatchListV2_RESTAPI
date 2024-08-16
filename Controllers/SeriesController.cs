using Microsoft.AspNetCore.Mvc;
using WatchListV2.Models;
using Swashbuckle.AspNetCore.Annotations;
using WatchListV2.DTO;
using Microsoft.AspNetCore.Authorization;
using WatchListV2.Constants;
namespace WatchListV2.Controllers
{
    //look into making sure pagination works, self, previous next, and last
    [Route("[controller]/[action]")]
    [ApiController]
    [Authorize(Roles = $"{RoleNames.User},{RoleNames.Administrator}")]

    public class SeriesController : ControllerBase
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(ISeriesService seriesService) =>
            _seriesService = seriesService;

        //Create
        [HttpPost(Name = "CreateSeries")]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [SwaggerOperation(
            Summary = "Creates a new Series",
            Description = "Creates a new series and stores it into the Database"
            )]
        public async Task<ActionResult<RestDTO<SeriesModel>>> Post([FromBody] SeriesDTO input)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _seriesService.CreateSeriesAsync(input);
                result.Links.Add(new LinkDTO(Url.Action(null, "Series", Request.Scheme)!, "self","POST"));

                return CreatedAtAction(nameof(Get), new { id = result.Data.SeriesID }, result);
            }
            catch (Exception ex) {
                var exceptionDetails = new ProblemDetails
                {
                    Detail = ex.Message,
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                };
                return StatusCode(StatusCodes.Status500InternalServerError, exceptionDetails);

            }
        }
        // GET
        [HttpGet(Name = "GetSeries")]
        [ResponseCache(CacheProfileName = "Any-60")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Gets a list of series",
            Description = "Retrieves a list of Series with paging, sorting, and filtering rules"
        )]
        public async Task<ActionResult<RestDTO<SeriesModel[]>>> Get([FromQuery] RequestDTO<SeriesDTO> input)
        {
            var result = await _seriesService.GetSeriesAsync(input);

            if (result.RecordCount == 0)
            {
                return NotFound(result);
            }

            result.Links.Add(new LinkDTO(Url.Action(null, "Series", new{ input.PageIndex, input.PageSize }, Request.Scheme)!, "self", "GET"));
            return Ok(result);
        }

        // PUT
        [HttpPut(Name = "UpdateSeries")]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [SwaggerOperation(
            Summary = "Updates a series",
            Description = "Updates series data related to input in the Database"
        )]
        public async Task<ActionResult<RestDTO<SeriesModel?>>> Put([FromBody] SeriesDTO model)
        {
            var result = await _seriesService.UpdateSeriesAsync(model);
            if(result.Data == null)
            {
                return NotFound(result);
            }
            result.Links.Add(new LinkDTO(Url.Action(null,"Series",new { id = model.Id}, Request.Scheme)!,"self","PUT"));
            return Ok(result);
        }

        // DELETE
        [HttpDelete("{id}", Name = "DeleteSeries")]
        [ResponseCache(CacheProfileName = "NoCache")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Deletes a series in the list",
            Description = "Deletes a series from the database based on SeriesID"
        )]
        public async Task<ActionResult<RestDTO<SeriesModel?>>> Delete(
            [SwaggerParameter("An id that represents SeriesID to be deleted")] int id)
        {
            var result = await _seriesService.DeleteSeriesAsync(id);

            if (result.Data == null)
            {
                return NotFound(result);
            }
            result.Links.Add(new LinkDTO(Url.Action(null, "Series", new { id }, Request.Scheme)!, "self", "DELETE"));
            return Ok(result);
        }
    }
}
