using System;

namespace CourseCatalogService.DTOs
{
    public record CreateCourseDto(
        string Name,
        string Description,
        int CreditCost,
        DateTime RegistrationDeadline,
        string Location,
        string? MeetingLink,
        DateTime StartDate,
        Guid LecturerId
    );

    public record UpdateCourseDto(
        string Name,
        string Description,
        int CreditCost,
        DateTime RegistrationDeadline,
        string Location,
        string? MeetingLink,
        DateTime StartDate,
        Guid LecturerId
    );

    public record CourseResponseDto(
        Guid Id,
        int? MonolithId,
        string Name,
        string Description,
        int CreditCost,
        DateTime RegistrationDeadline,
        string Location,
        string? MeetingLink,
        DateTime StartDate,
        Guid? LecturerId,
        string LecturerName
    );
}