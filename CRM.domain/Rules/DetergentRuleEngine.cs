using System;

namespace CRM.Domain.Rules
{
    /// <summary>
    /// Detergent and Load rules for laundry items.
    /// SINGLE SOURCE OF TRUTH for these business rules.
    ///
    /// Clothes:
    ///   Max load capacity = 8 kg
    ///   - 1.0 – 3.0 kg   → 3 g per load
    ///   - 3.1 – 7.0 kg   → 4 g per load
    ///   - 7.1 – 8.9 kg   → 5 g per load
    ///   - > 8.9 kg       → 5 g per load (extra loads add 5 g each)
    ///
    /// Beddings:
    ///   Max load capacity = 5 kg
    ///   - 1.0 – 3.0 kg   → 3 g per load
    ///   - 3.1 – 5.0 kg   → 5 g per load
    ///   - > 5.0 kg       → 5 g per load (extra loads add 5 g each)
    /// </summary>
    public static class DetergentRuleEngine
    {
        /// <summary>
        /// Max weight per single load.
        /// </summary>
        public static decimal GetLoadCapacity(string category)
        {
            return category?.Trim().ToLowerInvariant() switch
            {
                "clothes" => 8m,
                "beddings" => 5m,
                _ => throw new ArgumentException(
                    $"Unknown category '{category}'. Must be 'Clothes' or 'Beddings'.", nameof(category))
            };
        }

        /// <summary>
        /// Calculate detergent grams and load count.
        /// </summary>
        public static (decimal Detergent, decimal Load) Calculate(string category, decimal weightKg)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentException("Category is required.", nameof(category));

            if (weightKg <= 0)
                throw new ArgumentException("Weight must be greater than 0.", nameof(weightKg));

            return category.Trim().ToLowerInvariant() switch
            {
                "clothes" => CalculateClothes(weightKg),
                "beddings" => CalculateBeddings(weightKg),
                _ => throw new ArgumentException(
                    $"Unknown category '{category}'. Must be 'Clothes' or 'Beddings'.", nameof(category))
            };
        }

        // ============================================================
        // CLOTHES — max 8 kg per load
        // ============================================================
        private static (decimal, decimal) CalculateClothes(decimal weightKg)
        {
            const decimal loadCapacity = 8m;

            // How many loads?
            decimal loads = Math.Ceiling(weightKg / loadCapacity);
            if (loads < 1) loads = 1;

            // Detergent per load based on weight bracket
            decimal detergentPerLoad;
            if (weightKg >= 1.0m && weightKg <= 3.0m)
                detergentPerLoad = 3m;
            else if (weightKg > 3.0m && weightKg <= 7.0m)
                detergentPerLoad = 4m;
            else
                detergentPerLoad = 5m; // 7.1 kg and above

            // Total detergent scales with loads
            decimal totalDetergent = detergentPerLoad * loads;

            return (totalDetergent, loads);
        }

        // ============================================================
        // BEDDINGS — max 5 kg per load
        // ============================================================
        private static (decimal, decimal) CalculateBeddings(decimal weightKg)
        {
            const decimal loadCapacity = 5m;

            // How many loads?
            decimal loads = Math.Ceiling(weightKg / loadCapacity);
            if (loads < 1) loads = 1;

            // Detergent per load based on weight bracket
            decimal detergentPerLoad;
            if (weightKg >= 1.0m && weightKg <= 3.0m)
                detergentPerLoad = 3m;
            else
                detergentPerLoad = 5m; // 3.1 kg and above

            // Total detergent scales with loads
            decimal totalDetergent = detergentPerLoad * loads;

            return (totalDetergent, loads);
        }
    }
}