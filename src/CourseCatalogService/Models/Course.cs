using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CourseCatalogService.Models
{
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)] 
        public Guid Id { get; set; }

        // שדה עזר זמני לטובת המיגרציה מהמונוליט
        [BsonIgnoreIfNull]
        public int? MonolithId { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CreditCost { get; set; }
        public DateTime RegistrationDeadline { get; set; }

        // מסמך מוטמע (Embedded Document) - פותר את ה-1-to-1 הישן
        public CourseDetail? CourseDetail { get; set; }
    }
}