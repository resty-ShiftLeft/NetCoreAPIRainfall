using Microsoft.AspNetCore.Mvc;
using NetCoreAPIRainfall.Models;
using NetCoreAPIRainfall.Models.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Annotations;
using System;
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
        [SwaggerResponse(200, "A list of rainfall readings successfully retrieved", typeof(List<Models.rainfallReadingResponse>))]
        [SwaggerResponse(400, "Invalid request", typeof(List<error>))]
        [SwaggerResponse(404, "No readings found for the specified stationId", typeof(List<error>))]
        [SwaggerResponse(500, "Internal server error", typeof(List<error>))]
        public async Task<IActionResult> rainfall(string stationId, [FromQuery] int count = 10)
        {
            try
            {
                var externalUrl = $"https://environment.data.gov.uk/flood-monitoring/id/stations/{stationId}/readings?_sorted&_limit={count}";
                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        HttpResponseMessage message = await client.GetAsync(externalUrl);

                        if (!message.IsSuccessStatusCode)
                        {
                            var customError = new List<error>
                            {
                                new error
                                {
                                    message = ((int)message.StatusCode).ToString(),
                                    detail = new List<errorDetail>
                                    {
                                        new errorDetail
                                        {
                                            propertyName = message.StatusCode.ToString(),
                                            message = message.IsSuccessStatusCode ? string.Empty : "Details of invalid request property"
                                        }
                                    }
                                }
                            };

                            return message.StatusCode == System.Net.HttpStatusCode.NotFound
                                ? NotFound(customError)
                                : message.StatusCode == System.Net.HttpStatusCode.BadRequest
                                    ? BadRequest(customError)
                                    : new JsonResult(customError);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        var customError = new List<error>
                        {
                            new error
                            {
                                message = string.Empty,
                                detail = new List<errorDetail>
                                {
                                    new errorDetail
                                    {
                                        propertyName = string.Empty,
                                        message =  $"HTTP request error: {ex.Message}",
                                    }
                                }
                            }
                        };
                        Console.WriteLine($"HTTP request error: {ex.Message}");
                        return new JsonResult(customError);
                    }
                }

                var response = await _httpClient.GetStringAsync(externalUrl);
                var items = JsonConvert.DeserializeObject<JObject>(response)?["items"];

                if (items == null || items.Count() == 0)
                {
                    var notFoundError = CreateError("404", $"No readings found for the specified stationId {stationId}");
                    return NotFound(notFoundError);
                }

                var readingsRes = items?.Select(item => new Models.rainfallReadingResponse
                {
                    Readings = new List<Models.rainfallReading>
            {
                new Models.rainfallReading
                {
                    dateMeasured = DateTime.Parse(item["dateTime"].ToString()),
                    amountMeasured = decimal.Parse(item["value"].ToString()),
                }
            }
                }).ToList() ?? new List<Models.rainfallReadingResponse>();

                return new JsonResult(readingsRes);
            }
            catch (Exception ex)
            {
                var error = CreateError("500", "Internal server error", ex.Message);
                return new JsonResult(error);
            }

        }

        private List<error> CreateError(string errorCode, string errorMessage, string detail = null)
        {
            var customError = new List<error>
        {
            new error
            {
                message = errorCode,
                detail = new List<errorDetail>
                {
                    new errorDetail
                    {
                        propertyName = errorMessage,
                        message = detail
                    }
                }
            }
        };
            return customError;
        }
    }

}
