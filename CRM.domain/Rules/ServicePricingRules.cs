using System;
using System.Collections.Generic;
using System.Linq;

namespace CRM.Domain.Rules
{
    /// <summary>
    /// Service pricing rules — price per kg for each service type.
    /// </summary>
    public static class ServicePricingRules
    {
        public class ServiceInfo
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; } = string.Empty;
            public decimal PricePerKg { get; set; }
            public bool IsActive { get; set; } = true;
        }

        public static readonly List<ServiceInfo> Services = new()
        {
            new ServiceInfo { ServiceId = 1, ServiceName = "Full Service", PricePerKg = 100m },
            new ServiceInfo { ServiceId = 2, ServiceName = "Dry",          PricePerKg = 60m },
            new ServiceInfo { ServiceId = 3, ServiceName = "Wash",         PricePerKg = 50m },
            new ServiceInfo { ServiceId = 4, ServiceName = "Spin",         PricePerKg = 30m },
            new ServiceInfo { ServiceId = 5, ServiceName = "Spin and Dry", PricePerKg = 70m },
            new ServiceInfo { ServiceId = 6, ServiceName = "Fold",         PricePerKg = 25m }
        };

        public static ServiceInfo? GetService(int serviceId)
            => Services.FirstOrDefault(s => s.ServiceId == serviceId);

        public static ServiceInfo? GetService(string serviceName)
            => Services.FirstOrDefault(s =>
                s.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Calculates service line total = PricePerKg × Weight.
        /// </summary>
        public static decimal CalculateServiceLine(int serviceId, decimal weightKg)
        {
            var service = GetService(serviceId)
                ?? throw new InvalidOperationException($"Service {serviceId} not found.");

            return service.PricePerKg * weightKg;
        }
    }
}