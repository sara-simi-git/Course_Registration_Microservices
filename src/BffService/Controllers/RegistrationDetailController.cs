using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BffService.DTOs;

namespace BffService.Controllers
{
    [ApiController]
    [Route("api/bff/[controller]")]
    public class RegistrationDetailController : ControllerBase
    {
        private readonly HttpClient _registrationClient;
        private readonly HttpClient _catalogClient;

        public RegistrationDetailController(IHttpClientFactory httpClientFactory)
        {
            _registrationClient = httpClientFactory.CreateClient("registration");
            _catalogClient = httpClientFactory.CreateClient("catalog");
        }

        [HttpGet("{registrationId:int}")]
        public async Task<IActionResult> GetRegistrationDetail(int registrationId)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString("N");
            using var registrationRequest = new HttpRequestMessage(HttpMethod.Get, $"api/registration/{registrationId}");
            registrationRequest.Headers.Add("X-Correlation-ID", correlationId);
            var registrationResponse = await _registrationClient.SendAsync(registrationRequest);
            if (!registrationResponse.IsSuccessStatusCode)
                return StatusCode((int)registrationResponse.StatusCode, await registrationResponse.Content.ReadAsStringAsync());

            var registration = await registrationResponse.Content.ReadFromJsonAsync<RegistrationDto>();
            if (registration == null)
                return NotFound(new { message = "Registration not found." });

            using var courseRequest = new HttpRequestMessage(HttpMethod.Get, $"api/course/{registration.CourseId}");
            courseRequest.Headers.Add("X-Correlation-ID", correlationId);
            var courseResponse = await _catalogClient.SendAsync(courseRequest);
            if (!courseResponse.IsSuccessStatusCode)
                return StatusCode((int)courseResponse.StatusCode, await courseResponse.Content.ReadAsStringAsync());

            var course = await courseResponse.Content.ReadFromJsonAsync<CourseDto>();
            if (course == null)
                return NotFound(new { message = "Course not found." });

            var detail = new RegistrationDetailDto
            {
                RegistrationId = registration.RegistrationId,
                EmployeeId = registration.EmployeeId,
                CourseId = registration.CourseId,
                CourseName = registration.CourseName,
                CourseDescription = course.Description,
                CreditCost = course.CreditCost,
                RegistrationDate = registration.RegistrationDate,
                Status = registration.Status,
                AvailableCredits = registration.AvailableCredits,
                LecturerName = course.LecturerName
            };

            return Ok(detail);
        }
    }
}
