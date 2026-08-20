using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CourseCatalogService.DTOs;
using CourseCatalogService.Interfaces;

namespace CourseCatalogService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LecturerController : ControllerBase
    {
        private readonly ILecturerService _lecturerService;

        public LecturerController(ILecturerService lecturerService)
        {
            _lecturerService = lecturerService;
        }

        [HttpGet("lecturersList")]
        public async Task<ActionResult<IEnumerable<LecturerResponseDto>>> GetLecturersList()
        {
            return Ok(await _lecturerService.GetLecturersList());
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<LecturerResponseDto>> GetLecturerById(Guid id)
        {
            var lecturer = await _lecturerService.GetLecturerById(id);
            if (lecturer == null)
                return NotFound(new { message = $"Lecturer with ID {id} does not exist!" });
            return Ok(lecturer);
        }

        [HttpGet("SearchLecturersByName")]
        public async Task<ActionResult<IEnumerable<LecturerResponseDto>>> SearchLecturersByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "The search function must receive a name!" });

            return Ok(await _lecturerService.SearchLecturersByName(name));
        }

        [HttpPost]
        public async Task<ActionResult<LecturerResponseDto>> CreateLecturer([FromBody] LecturerCreateDto newLecturer)
        {
            var lecturer = await _lecturerService.CreateLecturer(newLecturer);
            return Ok(lecturer);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<LecturerResponseDto>> UpdateLecturer(Guid id, [FromBody] LecturerUpdateDto lecturer)
        {
            var updated = await _lecturerService.UpdateLecturer(id, lecturer);
            if (updated == null)
                return NotFound(new { message = $"Lecturer with ID {id} does not exist!" });
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteLecturer(Guid id)
        {
            var deleted = await _lecturerService.DeleteLecturer(id);
            if (!deleted)
                return NotFound(new { message = $"Lecturer with ID {id} does not exist!" });
            return Ok("Deleted!");
        }
    }
}