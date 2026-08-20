using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseCatalogService.DTOs;
using CourseCatalogService.Interfaces;
using CourseCatalogService.Models;

namespace CourseCatalogService.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly ILecturerRepository _lecturerRepository;
        private readonly StackExchange.Redis.IConnectionMultiplexer _redis;
        private readonly Microsoft.Extensions.Logging.ILogger<CourseService> _logger;

            public CourseService(ICourseRepository courseRepository, ILecturerRepository lecturerRepository, StackExchange.Redis.IConnectionMultiplexer redis, Microsoft.Extensions.Logging.ILogger<CourseService> logger)
            {
                _courseRepository = courseRepository;
                _lecturerRepository = lecturerRepository;
                _redis = redis;
                _logger = logger;
            }

        public async Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync()
        {
            var courses = await _courseRepository.GetAllCoursesAsync();
            return courses.Select(MapToResponseDto);
        }

        public async Task<CourseResponseDto?> GetCourseByIdAsync(Guid id)
        {
            var db = _redis.GetDatabase();
            var key = $"course:{id}";
            var cached = await db.StringGetAsync(key);
            if (cached.HasValue)
            {
                _logger.LogInformation("Cache HIT for course {CourseId}", id);
                try
                {
                    var dto = System.Text.Json.JsonSerializer.Deserialize<CourseResponseDto>(cached.ToString()!);
                    if (dto != null) return dto;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize cached course {CourseId}", id);
                }
            }

            _logger.LogInformation("Cache MISS for course {CourseId}", id);
            var course = await _courseRepository.GetCourseByIdAsync(id);
            if (course == null) return null;
            var mapped = MapToResponseDto(course);
            try
            {
                await db.StringSetAsync(key, System.Text.Json.JsonSerializer.Serialize(mapped), TimeSpan.FromMinutes(5));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to set cache for course {CourseId}", id);
            }

            return mapped;
        }

        public async Task<CourseResponseDto?> GetCourseWithEmployeesAsync(Guid id)
        {
            var course = await _courseRepository.GetCourseWithEmployeesAsync(id);
            return course != null ? MapToResponseDto(course) : null;
        }

        public async Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            // בדיקת שלמות נתונים אקטיבית מול קולקשן המרצים
            var lecturer = await _lecturerRepository.GetLecturerById(dto.LecturerId);
            if (lecturer == null)
            {
                throw new ArgumentException($"Data Integrity Error: Lecturer with ID {dto.LecturerId} does not exist in the system!");
            }

            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreditCost = dto.CreditCost,
                RegistrationDeadline = dto.RegistrationDeadline,
                CourseDetail = new CourseDetail
                {
                    Location = dto.Location,
                    MeetingLink = dto.MeetingLink,
                    StartDate = dto.StartDate,
                    LecturerId = dto.LecturerId,
                    LecturerName = lecturer.FullName // השם נלקח ישירות ובאופן מאובטח מה-DB
                }
            };

            var created = await _courseRepository.CreateCourseAsync(course);
            return MapToResponseDto(created);
        }

        public async Task<CourseResponseDto?> UpdateCourseAsync(Guid id, UpdateCourseDto dto)
        {
            var existing = await _courseRepository.GetCourseByIdAsync(id);
            if (existing == null) return null;

            // אימות קיום המרצה המעודכן
            var lecturer = await _lecturerRepository.GetLecturerById(dto.LecturerId);
            if (lecturer == null)
            {
                throw new ArgumentException($"Data Integrity Error: Lecturer with ID {dto.LecturerId} does not exist in the system!");
            }

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.CreditCost = dto.CreditCost;
            existing.RegistrationDeadline = dto.RegistrationDeadline;
            existing.CourseDetail = new CourseDetail
            {
                Location = dto.Location,
                MeetingLink = dto.MeetingLink,
                StartDate = dto.StartDate,
                LecturerId = dto.LecturerId,
                LecturerName = lecturer.FullName // עדכון השם המאומת
            };

            var updated = await _courseRepository.UpdateCourseAsync(existing);
            // invalidate cache for this course
            try
            {
                var db = _redis.GetDatabase();
                var key = $"course:{id}";
                await db.KeyDeleteAsync(key);
                _logger.LogInformation("Invalidated cache for course {CourseId}", id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate cache for course {CourseId}", id);
            }
            return updated != null ? MapToResponseDto(updated) : null;
        }

        public async Task<bool> DeleteCourseAsync(Guid id)
        {
            return await _courseRepository.DeleteCourseAsync(id);
        }

        private static CourseResponseDto MapToResponseDto(Course c) =>
            new CourseResponseDto(
                c.Id, 
                c.MonolithId, 
                c.Name, 
                c.Description, 
                c.CreditCost, 
                c.RegistrationDeadline,
                c.CourseDetail?.Location ?? string.Empty, 
                c.CourseDetail?.MeetingLink, 
                c.CourseDetail?.StartDate ?? DateTime.MinValue,
                c.CourseDetail?.LecturerId, 
                c.CourseDetail?.LecturerName ?? string.Empty
            );
    }
}