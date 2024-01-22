using Microsoft.AspNetCore.Mvc;
using NetCoreAPIRainfall.Models.Responses;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace NetCoreAPIRainfall.Controllers
{
    public class RainfallController : Controller
    {
        private readonly HttpClient _httpClient;

        public RainfallController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet("/rainfall/id/{stationId}/readings")]
        public async Task<IActionResult> rainfall(string stationId, [FromQuery] int count = 10)
        {
            var externalUrl = "https://environment.data.gov.uk/flood-monitoring/id/stations/" + stationId + "/readings?_sorted&_limit=" + count;
            var response = await _httpClient.GetStringAsync(externalUrl);

            return Ok(response);
        }


        /// <summary>
        /// Get rainfall readings by station Id
        /// </summary>
        /// <remarks>
        /// Retrieve the latest readings for the specified stationId
        /// </remarks>
        /// <param name="stationId">The ID of the station for which to retrieve readings.</param>
        /// 

        [SwaggerResponse(200, "A list of rainfall readings successfully retrieved", typeof(rainfallReadingResponse))]
        [SwaggerResponse(400, "Invalid request", typeof(errorResponse))]
        [SwaggerResponse(404, "No readings found for the specified stationId", typeof(errorResponse))]
        [SwaggerResponse(500, "Internal server error", typeof(errorResponse))]
        [HttpGet("/get-rainfall")]
        public async Task<IActionResult> get_rainfall_by_stationId([Required] string stationId)
        {
            var externalUrl = "https://environment.data.gov.uk/flood-monitoring/id/stations/" + stationId + "/readings?_sorted";
            var response = await _httpClient.GetStringAsync(externalUrl);

            return Ok(response);
        }
    }
}
