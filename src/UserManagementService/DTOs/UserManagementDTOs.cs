using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs
{
    public class AddCreditsDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Credits amount must be positive.")]
        public int Amount { get; set; }
    }
}
