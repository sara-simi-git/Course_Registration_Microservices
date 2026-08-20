using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CourseCatalogService.Data;
using CourseCatalogService.Interfaces;
using CourseCatalogService.Models;

namespace CourseCatalogService.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly MongoDbContext _context;

        public CourseRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.Find(_ => true).ToListAsync();
        }

        public async Task<Course?> GetCourseByIdAsync(Guid id)
        {
            return await _context.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Course?> GetCourseWithEmployeesAsync(Guid id)
        {
            // ישות Employee מנוהלת ב-CapacityAndCreditService, לכן המתודה מחזירה את הקורס בלבד לטובת תאימות פונקציונלית
            return await _context.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            await _context.Courses.InsertOneAsync(course);
            return course;
        }

        public async Task<Course?> UpdateCourseAsync(Course course)
        {
            var result = await _context.Courses.ReplaceOneAsync(c => c.Id == course.Id, course);
            return result.MatchedCount > 0 ? course : null;
        }

        public async Task<bool> UpdateLecturerNameForCoursesAsync(Guid lecturerId, string lecturerName)
        {
            var filter = Builders<Course>.Filter.Eq(c => c.CourseDetail!.LecturerId, lecturerId);
            var updateDefinition = Builders<Course>.Update.Set(c => c.CourseDetail!.LecturerName, lecturerName);
            var result = await _context.Courses.UpdateManyAsync(filter, updateDefinition);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteCourseAsync(Guid id)
        {
            var result = await _context.Courses.DeleteOneAsync(c => c.Id == id);
            return result.DeletedCount > 0;
        }
    }
}