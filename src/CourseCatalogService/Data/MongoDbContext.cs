using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using CourseCatalogService.Models;

namespace CourseCatalogService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            var databaseName = configuration.GetValue<string>("DatabaseName") ?? "CourseCatalogDb";
            
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<Course> Courses => 
            _database.GetCollection<Course>("Courses");

        public IMongoCollection<Lecturer> Lecturers => 
            _database.GetCollection<Lecturer>("Lecturers");
    }
}