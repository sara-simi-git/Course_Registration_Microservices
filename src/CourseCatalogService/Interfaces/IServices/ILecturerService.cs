using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseCatalogService.DTOs;

namespace CourseCatalogService.Interfaces
{
    public interface ILecturerService
    {
        Task<IEnumerable<LecturerResponseDto>> GetLecturersList();
        Task<LecturerResponseDto?> GetLecturerById(Guid id);
        Task<IEnumerable<LecturerResponseDto>> SearchLecturersByName(string name);
        Task<LecturerResponseDto> CreateLecturer(LecturerCreateDto newLecturer);
        Task<LecturerResponseDto?> UpdateLecturer(Guid id, LecturerUpdateDto lecturer);
        Task<bool> DeleteLecturer(Guid id);
    }
}