using RegistrationService.DTOs;
using RegistrationService.Interfaces;
using RegistrationService.Models;
using RegistrationService.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistrationService.Enums;

namespace RegistrationService.Services
{
    public class CourseRegistrationService : IRegistrationService
    {
        private readonly ICourseRegistrationRepository _registrationRepository;
        private readonly IEventBus _eventBus;

        public CourseRegistrationService(
            ICourseRegistrationRepository registrationRepository,
            IEventBus eventBus)
        {
            _registrationRepository = registrationRepository;
            _eventBus = eventBus;
        }

        public async Task<CourseRegistrationResponseDto?> GetRegistrationByIdAsync(int id)
        {
            var registration = await _registrationRepository.GetByIdAsync(id);
            if (registration == null) return null;

            return MapToResponseDto(registration);
        }

        public async Task<IEnumerable<CourseRegistrationResponseDto>> GetEmployeeRegistrationsAsync(int employeeId)
        {
            var registrations = await _registrationRepository.GetCoursesByEmployeeIdAsync(employeeId);
            return registrations.Select(MapToResponseDto);
        }

        public async Task<IEnumerable<CourseRegistrationResponseDto>> GetCourseRegistrationsAsync(Guid courseId)
        {
            var registrations = await _registrationRepository.GetEmployeesByCourseIdAsync(courseId);
            return registrations.Select(MapToResponseDto);
        }

        public async Task<CourseRegistrationResponseDto?> RegisterToCourseAsync(EmployeeRegistrationDto dto, string? correlationId = null)
        {
            var existing = await _registrationRepository.GetByEmployeeAndCourseAsync(dto.EmployeeId, dto.CourseId);
            if (existing != null) return null;

            var registration = new CourseRegistration
            {
                EmployeeId = dto.EmployeeId,
                CourseId = dto.CourseId,
                RegistrationDate = DateTime.UtcNow,
                Status = CourseRegistrationStatus.Active // נוצר כ-Active כברירת מחדל בקוד המקור
            };

            await _registrationRepository.AddRegistrationAsync(registration);

            // הפצת אירוע אסינכרוני כדי ש-CapacityAndCreditService יאמת ויקצה משאבים
            var orderEvent = new RegistrationService.Events.OrderPlaced(
                registration.Id,
                registration.EmployeeId,
                registration.CourseId,
                registration.RegistrationDate,
                string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId
            );
            await _eventBus.PublishAsync(orderEvent);

            return MapToResponseDto(registration);
        }

        public async Task<bool> ChangeRegistrationStatusAsync(int registrationId, int status)
        {
            if (!Enum.IsDefined(typeof(CourseRegistrationStatus), status))
                return false;

            var currentStatus = (CourseRegistrationStatus)status;
            var updatedStatus = await _registrationRepository.ChangeStatusCourseAsync(registrationId, currentStatus);
            
            // אם הסטטוס עודכן ל-Completed, ניתן להפיץ אירוע ייעודי במידת הצורך
            return true;
        }

        public async Task<bool> CancelRegistrationAsync(int registrationId)
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null) return false;

            registration.Status = CourseRegistrationStatus.Canceled;
            await _registrationRepository.SaveChangesAsync();

            // שליחת הודעה ל-Event Bus לשחרור המקום והקרדיט ב-CapacityAndCreditService
            var cancelEvent = new RegistrationCancelled(
                registration.Id,
                registration.EmployeeId,
                registration.CourseId,
                DateTime.UtcNow
            );
            await _eventBus.PublishAsync(cancelEvent);

            return true;
        }

        private CourseRegistrationResponseDto MapToResponseDto(CourseRegistration registration)
        {
            return new CourseRegistrationResponseDto
            {
                RegistrationId = registration.Id,
                EmployeeId = registration.EmployeeId,
                CourseId = registration.CourseId,
                RegistrationDate = registration.RegistrationDate,
                Status = (int)registration.Status
            };
        }
    }
}