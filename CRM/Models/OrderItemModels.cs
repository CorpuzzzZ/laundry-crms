using System.Collections.Generic;

namespace CRM.WinForms.Models
{
    public class ClotheTypeLookup
    {
        public int ClotheTypeId { get; set; }
        public string ClotheTypeName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class LaundryServiceLookup
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal PricePerKg { get; set; }
    }

    public class ChemicalAddOnModel
    {
        public int ChemicalAddOnId { get; set; }
        public string AddOnName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public bool IsSelected => Quantity > 0;
    }

    public class MachineAddOnModel
    {
        public int MachineAddOnId { get; set; }
        public string AddOnName { get; set; } = string.Empty;
        public int DryingMinutes { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public bool IsSelected => Quantity > 0;
    }

    public class OrderItemRequest
    {
        public int ClotheTypeId { get; set; }
        public string ClotheTypeName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public decimal DetergentGrams { get; set; }
        public decimal LoadCount { get; set; }
        public List<ChemicalAddOnModel> ChemicalAddOns { get; set; } = new();
        public List<MachineAddOnModel> MachineAddOns { get; set; } = new();
        public decimal ServiceLineTotal { get; set; }
        public decimal ChemicalTotal { get; set; }
        public decimal MachineTotal { get; set; }
        public decimal LineTotal { get; set; }
    }
}