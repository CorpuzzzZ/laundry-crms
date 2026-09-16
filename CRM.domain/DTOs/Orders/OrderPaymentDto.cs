using System;
using System.ComponentModel.DataAnnotations;

namespace CRM.Domain.DTOs.Orders
{
    public class OrderPaymentDto
    {
        public int OrderPaymentId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class CreateOrderPaymentDto
    {
        [Required]
        public int PaymentMethodId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
    }
}