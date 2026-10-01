using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Orders
{
    public class OrderStatusChangeDto
    {
        [Required]
        public int StatusId { get; set; }

        public string? Notes { get; set; }
        public string? ChangedByName { get; set; }
    }
}
