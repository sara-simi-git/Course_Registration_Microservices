using System;

namespace InventoryService.Models
{
    public class CourseInventory
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public int MaxPlaces { get; set; }
        public int ReservedPlaces { get; set; }

        public int PlacesLeft => MaxPlaces - ReservedPlaces;
    }
}
