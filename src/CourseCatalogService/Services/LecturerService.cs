using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseCatalogService.DTOs;
using CourseCatalogService.Interfaces;
using CourseCatalogService.Models;

namespace CourseCatalogService.Services
{
    public class LecturerService : ILecturerService
    {
        private readonly ILecturerRepository _lecturerRepository;
        private readonly ICourseRepository _courseRepository;

        public LecturerService(ILecturerRepository lecturerRepository, ICourseRepository courseRepository)
        {
            _lecturerRepository = lecturerRepository;
            _courseRepository = courseRepository;
        }

        public async Task<IEnumerable<LecturerResponseDto>> GetLecturersList()
        {
            var lecturers = await _lecturerRepository.GetLecturersList();
            return lecturers.Select(MapToResponseDto);
        }

        public async Task<LecturerResponseDto?> GetLecturerById(Guid id)
        {
            var lecturer = await _lecturerRepository.GetLecturerById(id);
            return lecturer != null ? MapToResponseDto(lecturer) : null;
        }

        public async Task<IEnumerable<LecturerResponseDto>> SearchLecturersByName(string name)
        {
            var lecturers = await _lecturerRepository.SearchLecturersByName(name);
            return lecturers.Select(MapToResponseDto);
        }

        public async Task<LecturerResponseDto> CreateLecturer(LecturerCreateDto dto)
        {
            var lecturer = new Lecturer
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Cost = dto.Cost
            };

            var created = await _lecturerRepository.CreateLecturer(lecturer);
            return MapToResponseDto(created);
        }

        public async Task<LecturerResponseDto?> UpdateLecturer(Guid id, LecturerUpdateDto dto)
        {
            var lecturer = new Lecturer
            {
                Id = id,
                FullName = dto.FullName,
                Email = dto.Email,
                Phone = dto.Phone,
                CompanyName = dto.CompanyName,
                Cost = dto.Cost
            };

            var updated = await _lecturerRepository.UpdateLecturer(lecturer);
            if (updated == null)
            {
                return null;
            }

            await _courseRepository.UpdateLecturerNameForCoursesAsync(updated.Id, updated.FullName);
            return MapToResponseDto(updated);
        }

        public async Task<bool> DeleteLecturer(Guid id)
        {
            return await _lecturerRepository.DeleteLecturer(id);
        }

        private static LecturerResponseDto MapToResponseDto(Lecturer l) =>
            new LecturerResponseDto(l.Id, l.FullName, l.Email, l.Phone, l.CompanyName, l.Cost);
    }
}