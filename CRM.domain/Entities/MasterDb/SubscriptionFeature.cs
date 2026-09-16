namespace CRM.Domain.Entities.MasterDb
{
    public class SubscriptionFeature
    {
        public int SubscriptionFeatureId { get; set; }
        public int PlanId { get; set; }
        public string FeatureKey { get; set; } = string.Empty;
        public string? FeatureValue { get; set; }
        public bool IsEnabled { get; set; } = true;

        public virtual SubscriptionPlan? Plan { get; set; }
    }
}
