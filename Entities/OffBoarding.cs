
using System;
namespace hr_crm.Entities
{
    public class OffBoarding
    {
        
            public int Id { get; set; }
            public int UserId { get; set; }
            public DateTime ResignationDate { get; set; }
            public DateTime LastWorkingDate { get; set; }
            public string Reason { get; set; }
            public string KnowledgeTransferStatus { get; set; }
            public string AssetReturnStatus { get; set; }
            public string ExitInterviewStatus { get; set; }
            public string OverallStatus { get; set; }
           public bool AccountDeactivation { get; set; }

    }
}

