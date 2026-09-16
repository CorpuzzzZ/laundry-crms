using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain.Rules
{
    /// <summary>
    /// Add-on master list (chemical and machine) with unit prices.
    /// </summary>
    public static class AddOnRules
    {
        public class AddOnInfo
        {
            public int AddOnId { get; set; }
            public string AddOnName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public string Type { get; set; } = "Chemical"; // "Chemical" or "Machine"
            public int? DryingMinutes { get; set; }
        }

        public static readonly List<AddOnInfo> ChemicalAddOns = new()
        {
            new AddOnInfo { AddOnId = 1, AddOnName = "Fabcon",    UnitPrice = 20m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 2, AddOnName = "Fabcon 1",  UnitPrice = 25m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 3, AddOnName = "Fabcon 2",  UnitPrice = 30m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 4, AddOnName = "Fabcon 3",  UnitPrice = 35m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 5, AddOnName = "Detergent", UnitPrice = 15m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 6, AddOnName = "Cologne",   UnitPrice = 25m, Type = "Chemical" },
            new AddOnInfo { AddOnId = 7, AddOnName = "Colorsafe", UnitPrice = 30m, Type = "Chemical" }
        };

        public static readonly List<AddOnInfo> MachineAddOns = new()
        {
            new AddOnInfo { AddOnId = 1, AddOnName = "Drying 10 mins", UnitPrice = 10m, Type = "Machine", DryingMinutes = 10 },
            new AddOnInfo { AddOnId = 2, AddOnName = "Drying 20 mins", UnitPrice = 20m, Type = "Machine", DryingMinutes = 20 },
            new AddOnInfo { AddOnId = 3, AddOnName = "Drying 30 mins", UnitPrice = 30m, Type = "Machine", DryingMinutes = 30 }
        };

        public static AddOnInfo? GetChemical(int addOnId)
            => ChemicalAddOns.FirstOrDefault(a => a.AddOnId == addOnId);

        public static AddOnInfo? GetMachine(int addOnId)
            => MachineAddOns.FirstOrDefault(a => a.AddOnId == addOnId);

        public static decimal CalculateChemicalTotal(IEnumerable<(int AddOnId, int Qty)> items)
        {
            decimal total = 0;
            foreach (var (id, qty) in items)
            {
                var addOn = GetChemical(id);
                if (addOn != null) total += addOn.UnitPrice * qty;
            }
            return total;
        }

        public static decimal CalculateMachineTotal(IEnumerable<(int AddOnId, int Qty)> items)
        {
            decimal total = 0;
            foreach (var (id, qty) in items)
            {
                var addOn = GetMachine(id);
                if (addOn != null) total += addOn.UnitPrice * qty;
            }
            return total;
        }
    }
}