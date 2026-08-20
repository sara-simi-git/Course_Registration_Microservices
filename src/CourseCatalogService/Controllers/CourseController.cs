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
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet("coursesList")]
        public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetAllCourses()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        // [HttpGet("availableCourses")]
        // public async Task<ActionResult<IEnumerable<CourseResponseDto>>> GetAvailableCourses()
        // {
        //     var courses = await _courseService.GetAvailableCoursesAsync();
        //     return Ok(courses);
        // }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CourseResponseDto>> GetCourseById(Guid id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound(new { message = $"Course with ID {id} does not exist!" });
            return Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<CourseResponseDto>> CreateCourse([FromBody] CreateCourseDto newCourse)
        {
            try
            {
                var createdCourse = await _courseService.CreateCourseAsync(newCourse);
                return Ok(createdCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CourseResponseDto>> UpdateCourse(Guid id, [FromBody] UpdateCourseDto updCourse)
        {
            try
            {
                var updatedCourse = await _courseService.UpdateCourseAsync(id, updCourse);
                if (updatedCourse == null)
                    return NotFound(new { message = $"Course with ID {id} does not exist!" });
                return Ok(updatedCourse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCourse(Guid id)
        {
            var isDeleted = await _courseService.DeleteCourseAsync(id);
            if (!isDeleted)
                return NotFound(new { message = $"Course with ID {id} does not exist!" });
            return Ok("Deleted!");
        }
    }
}