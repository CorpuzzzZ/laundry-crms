using System;

namespace CRM.Domain.Entities.TenantDb
{
    public class OrderPayment
    {
        public int OrderPaymentId { get; set; }
        public int OrderId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Completed";
        public string? Notes { get; set; }

        public virtual Order? Order { get; set; }
        public virtual PaymentMethod? PaymentMethod { get; set; }
    }
}
