using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using CourseCatalogService.Data;
using CourseCatalogService.Interfaces;
using CourseCatalogService.Models;
using MongoDB.Bson;

namespace CourseCatalogService.Repositories
{
    public class LecturerRepository : ILecturerRepository
    {
        private readonly MongoDbContext _context;

        public LecturerRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Lecturer?> GetLecturerById(Guid id)
        {
            return await _context.Lecturers.Find(l => l.Id == id).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Lecturer>> GetLecturersList()
        {
            return await _context.Lecturers.Find(_ => true).ToListAsync();
        }

        public async Task<IEnumerable<Lecturer>> SearchLecturersByName(string name)
        {
            var filter = Builders<Lecturer>.Filter.Regex(l => l.FullName, new BsonRegularExpression(name, "i"));
            return await _context.Lecturers.Find(filter).ToListAsync();
        }

        public async Task<Lecturer> CreateLecturer(Lecturer lecturer)
        {
            await _context.Lecturers.InsertOneAsync(lecturer);
            return lecturer;
        }

        public async Task<Lecturer?> UpdateLecturer(Lecturer lecturer)
        {
            var result = await _context.Lecturers.ReplaceOneAsync(l => l.Id == lecturer.Id, lecturer);
            return result.MatchedCount > 0 ? lecturer : null;
        }

        public async Task<bool> DeleteLecturer(Guid id)
        {
            var result = await _context.Lecturers.DeleteOneAsync(l => l.Id == id);
            return result.DeletedCount > 0;
        }
    }
}