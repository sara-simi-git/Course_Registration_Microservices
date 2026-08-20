using System;

namespace RegistrationService.Events
{
    public record RegistrationRequested(int RegistrationId, int EmployeeId, Guid CourseId, DateTime RegistrationDate);
    public record RegistrationCancelled(int RegistrationId, int EmployeeId, Guid CourseId, DateTime CancelledAt);
}
