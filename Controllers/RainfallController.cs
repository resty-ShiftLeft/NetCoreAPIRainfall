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
        public async Task<IActionResult> rainfall(string stationId, [FromQuery] int count = 10)
        {
            var externalUrl = "https://environment.data.gov.uk/flood-monitoring/id/stations/" + stationId + "/readings?_sorted&_limit=" + count;
            var response = await _httpClient.GetStringAsync(externalUrl);

            var jsonObject_response = JsonConvert.DeserializeObject<JObject>(response);
            var items = jsonObject_response["items"];


            var _readings_res = new List<Models.rainfallReadingResponse>();

            foreach (var item in items)
            {
                _readings_res.Add(new Models.rainfallReadingResponse
                {
                    Readings = new List<Models.rainfallReading>()
                    {
                        new Models.rainfallReading
                        {
                            dateMeasured = DateTime.Parse(item["dateTime"].ToString()),
                            amountMeasured = decimal.Parse(item["value"].ToString()),
                        }
                    }
                });
            }


            return new JsonResult(_readings_res);
        }


        /// <summary>
        /// Get rainfall readings by station Id
        /// </summary>
        /// <remarks>
        /// Retrieve the latest readings for the specified stationId
        /// </remarks>
        /// <param name="stationId">The ID of the station for which to retrieve readings.</param>
        /// 

        [SwaggerResponse(200, "A list of rainfall readings successfully retrieved", typeof(Models.Responses.rainfallReadingResponse))]
        [SwaggerResponse(400, "Invalid request", typeof(error))]
        [SwaggerResponse(404, "No readings found for the specified stationId", typeof(error))]
        [SwaggerResponse(500, "Internal server error", typeof(error))]
        [HttpGet("/get-rainfall")]
        public async Task<IActionResult> get_rainfall_by_stationId([Required] string stationId)
        {
            var externalUrl = "https://environment.data.gov.uk/flood-monitoring/id/stations/" + stationId + "/readings?_sorted";
            var response = await _httpClient.GetStringAsync(externalUrl);

            var jsonObject_response = JsonConvert.DeserializeObject<JObject>(response);
            var items = jsonObject_response["items"];

            var _readings_res = new List<Models.rainfallReadingResponse>();
            var _custom_error = new List<error>();
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage message = await client.GetAsync(externalUrl);

                    if (message.StatusCode == System.Net.HttpStatusCode.NotFound || items.Count() == 0)
                    {
                        _custom_error.Add(new error
                        {
                            message = "404",
                            detail = new List<errorDetail>()
                            {
                                new errorDetail
                                {
                                    propertyName = "Not Found",
                                    message = "No readings found for the specified stationId"
                                }
                            }
                        });

                        return NotFound(_custom_error);
                    }
                    else if (message.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                    {
                        _custom_error.Add(new error
                        {
                            message = "500",
                            detail = new List<errorDetail>()
                            {
                                new errorDetail
                                {
                                    propertyName = "Internal server error",
                                    message = ""
                                }
                            }
                        });

                        return new JsonResult(_custom_error);
                    }
                    else if (message.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        _custom_error.Add(new error
                        {
                            message = "400",
                            detail = new List<errorDetail>()
                            {
                                new errorDetail
                                {
                                    propertyName = "Invalid request",
                                    message = "Details of invalid request property"
                                }
                            }
                        });

                        return BadRequest(_custom_error);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {message.StatusCode}");
                    }
                }
                catch (HttpRequestException ex)
                {
                    _custom_error.Add(new error
                    {
                        message = "",
                        detail = new List<errorDetail>()
                            {
                                new errorDetail
                                {
                                    propertyName = "",
                                    message =  $"HTTP request error: {ex.Message}",
                                }
                            }
                    });
                    Console.WriteLine($"HTTP request error: {ex.Message}");
                }
            }

            
            foreach (var item in items)
            {
                _readings_res.Add(new Models.rainfallReadingResponse
                {
                    Readings = new List<Models.rainfallReading>()
                    {
                        new Models.rainfallReading
                        {
                            dateMeasured = DateTime.Parse(item["dateTime"].ToString()),
                            amountMeasured = decimal.Parse(item["value"].ToString()),
                        }
                    }
                });
            }
            

            return new JsonResult(_readings_res);
        }
    }
}
