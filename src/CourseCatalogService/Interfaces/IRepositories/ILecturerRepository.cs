using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseCatalogService.Models;

namespace CourseCatalogService.Interfaces
{
    public interface ILecturerRepository
    {
        Task<Lecturer?> GetLecturerById(Guid id);
        Task<IEnumerable<Lecturer>> GetLecturersList();
        Task<IEnumerable<Lecturer>> SearchLecturersByName(string name);
        Task<Lecturer> CreateLecturer(Lecturer lecturer);
        Task<Lecturer?> UpdateLecturer(Lecturer lecturer);
        Task<bool> DeleteLecturer(Guid id);
    }
}