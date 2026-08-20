using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CourseCatalogService.Models
{
    public class Lecturer
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
    }
}