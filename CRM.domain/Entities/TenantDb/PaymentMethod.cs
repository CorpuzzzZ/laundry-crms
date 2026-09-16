using System.Collections.Generic;

namespace CRM.Domain.Entities.TenantDb
{
    public class PaymentMethod
    {
        public int PaymentMethodId { get; set; }
        public string MethodCode { get; set; } = string.Empty;
        public string MethodName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        public virtual ICollection<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();
    }
}
