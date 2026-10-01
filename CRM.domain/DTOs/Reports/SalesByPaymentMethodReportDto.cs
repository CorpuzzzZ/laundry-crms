using System;
using System.Collections.Generic;

namespace CRM.Domain.DTOs.Reports
{
    public class SalesByPaymentMethodReportDto
    {
        public DateTime FromUtc { get; set; }
        public DateTime ToUtc { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPayments { get; set; }
        public List<SalesByPaymentMethodRow> Rows { get; set; } = new();
    }

    public class SalesByPaymentMethodRow
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string MethodCode { get; set; } = string.Empty;
        public int PaymentCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPct { get; set; }
    }
}
