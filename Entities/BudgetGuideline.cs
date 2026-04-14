namespace hr_crm.Entities;

public class BudgetGuideline
{
    public int BudgetGuidelineId { get; set; }

    public decimal MaxAnnualBudget { get; set; }

    public decimal MaxTrainingPercentage { get; set; }

    public decimal MaxResourcePercentage { get; set; }
}