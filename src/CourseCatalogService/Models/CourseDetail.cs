using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CourseCatalogService.Models
{
    public class CourseDetail
    {
        public string Location { get; set; } = string.Empty;
        public string? MeetingLink { get; set; }
        public DateTime StartDate { get; set; }
        
        [BsonRepresentation(BsonType.String)]
        public Guid? LecturerId { get; set; }
        
        // Denormalized property - נשמר כאן כדי לחסוך שליפות מורכבות בעת הצגת הקטלוג
        public string LecturerName { get; set; } = string.Empty;
    }
}