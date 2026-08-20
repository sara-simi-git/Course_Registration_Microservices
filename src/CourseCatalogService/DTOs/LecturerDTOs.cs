using System;

namespace CourseCatalogService.DTOs
{
    public record LecturerCreateDto(
        string FullName,
        string Email,
        string Phone,
        string CompanyName,
        decimal Cost
    );

    public record LecturerUpdateDto(
        string FullName,
        string Email,
        string Phone,
        string CompanyName,
        decimal Cost
    );

    public record LecturerResponseDto(
        Guid Id,
        string FullName,
        string Email,
        string Phone,
        string CompanyName,
        decimal Cost
    );
}