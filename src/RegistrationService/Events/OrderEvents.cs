using System;

namespace RegistrationService.Events
{
    public record OrderPlaced(int RegistrationId, int EmployeeId, Guid CourseId, DateTime PlacedAt, string CorrelationId);
    public record OrderConfirmed(int RegistrationId, Guid CourseId, DateTime ConfirmedAt, string CorrelationId);
    public record OrderCanceled(int RegistrationId, Guid CourseId, DateTime CanceledAt, string CorrelationId);
}
