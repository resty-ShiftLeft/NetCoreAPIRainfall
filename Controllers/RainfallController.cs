using Microsoft.AspNetCore.Mvc;
using NetCoreAPIRainfall.Models;
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


        [HttpGet("/get-rainfall")]
        [ProducesResponseType(typeof(List<rainfallReadingResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> get_rainfall_by_stationId([Required] string stationId)
        {
            // Your implementation to retrieve rainfall readings
            var externalUrl = "https://environment.data.gov.uk/flood-monitoring/id/stations/" + stationId + "/readings?_sorted";
            var response = await _httpClient.GetStringAsync(externalUrl);

            return Ok(response);
        }
    }
}
