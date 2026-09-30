using System;

namespace CRM.WinForms.Models
{
    public class LoyaltySettingModel
    {
        public int LoyaltySettingId { get; set; }
        public decimal PointsPerOrder { get; set; }
        public decimal PointsPerDollar { get; set; }
        public decimal RedeemPointsRequired { get; set; }
        public decimal RedeemDiscountAmount { get; set; }
        public int? PointsExpiryDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateLoyaltySettingRequest
    {
        public decimal PointsPerOrder { get; set; }
        public decimal PointsPerDollar { get; set; }
        public decimal RedeemPointsRequired { get; set; }
        public decimal RedeemDiscountAmount { get; set; }
        public int? PointsExpiryDays { get; set; }
        public bool IsActive { get; set; }
    }

    public class LoyaltyTierModel
    {
        public int LoyaltyTierId { get; set; }
        public string TierName { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PointsMultiplier { get; set; }
        public string? BenefitsJSON { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }

        public string RangeText => MaxPoints.HasValue
            ? $"{MinPoints} - {MaxPoints}"
            : $"{MinPoints}+";
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string DiscountText => $"{DiscountPercentage:0.##}%";
        public string MultiplierText => $"{PointsMultiplier:0.##}x";
    }

    public class CreateLoyaltyTierRequest
    {
        public string TierName { get; set; } = string.Empty;
        public int MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PointsMultiplier { get; set; } = 1.0m;
        public string? BenefitsJSON { get; set; }
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateLoyaltyTierRequest : CreateLoyaltyTierRequest { }
}

namespace CRM.WinForms.Models
{
    // Envelope wrappers matching the API's { success, message, data } shape
    public class LoyaltySettingEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoyaltySettingModel? Data { get; set; }
    }

    public class LoyaltyTierListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public System.Collections.Generic.List<LoyaltyTierModel>? Data { get; set; }
    }

    public class LoyaltyTierEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoyaltyTierModel? Data { get; set; }
    }
}

namespace CRM.WinForms.Models
{
    public class LoyaltyCustomerModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public int? LoyaltyTierId { get; set; }
        public string? TierName { get; set; }
        public int CurrentPoints { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsRedeemed { get; set; }
        public decimal LifetimeSpend { get; set; }
        public System.DateTime? LastActivityDate { get; set; }
        public bool IsActive { get; set; }

        public string TierDisplay => string.IsNullOrWhiteSpace(TierName) ? "—" : TierName!;
        public string LifetimeSpendText => $"PHP {LifetimeSpend:0.00}";
        public string LastActivityText => LastActivityDate.HasValue
            ? LastActivityDate.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "—";
    }

    public class LoyaltyTransactionModel
    {
        public int LoyaltyTransactionId { get; set; }
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string TransactionType { get; set; } = string.Empty;
        public int PointsChange { get; set; }
        public int PointsBalance { get; set; }
        public string? Description { get; set; }
        public string? Reference { get; set; }
        public System.DateTime TransactionDate { get; set; }

        public string PointsChangeText => PointsChange >= 0
            ? $"+{PointsChange}"
            : PointsChange.ToString();
        public string DateText => TransactionDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
    }

    public class LoyaltyCustomerListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public System.Collections.Generic.List<LoyaltyCustomerModel>? Data { get; set; }
    }

    public class LoyaltyTransactionListEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public System.Collections.Generic.List<LoyaltyTransactionModel>? Data { get; set; }
    }
}

namespace CRM.WinForms.Models
{
    public class LoyaltyCustomerEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public LoyaltyCustomerModel? Data { get; set; }
    }
}

namespace CRM.WinForms.Models
{
    public class RedeemableInfoModel
    {
        public int CustomerId { get; set; }
        public int CurrentPoints { get; set; }
        public int PointsRequired { get; set; }
        public decimal DiscountPerBatch { get; set; }
        public int RedeemableBatches { get; set; }
        public int MaxRedeemablePoints { get; set; }
        public decimal MaxDiscount { get; set; }
        public bool CanRedeem { get; set; }
    }

    public class RedeemableInfoEnvelope
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public RedeemableInfoModel? Data { get; set; }
    }

    public class LoyaltyTierPdfMeta
    {
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public string UploadedBy { get; set; } = string.Empty;

        public string FormattedSize
        {
            get
            {
                if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
                if (FileSizeBytes < 1024 * 1024) return $"{(FileSizeBytes / 1024.0):F1} KB";
                return $"{(FileSizeBytes / (1024.0 * 1024.0)):F2} MB";
            }
        }
    }
}
