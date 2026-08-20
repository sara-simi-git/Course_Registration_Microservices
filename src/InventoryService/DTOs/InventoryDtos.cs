using System;

namespace InventoryService.DTOs
{
    public class CreateInventoryDto
    {
        public Guid CourseId { get; set; }
        public int MaxPlaces { get; set; }
    }

    public class UpdateCapacityDto
    {
        public Guid CourseId { get; set; }
        public int MaxPlaces { get; set; }
    }

    public class InventoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public int MaxPlaces { get; set; }
        public int ReservedPlaces { get; set; }
        public int PlacesLeft { get; set; }
    }

    public class SeatOperationDto
    {
        public Guid CourseId { get; set; }
    }
}
