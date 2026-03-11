using System;
using System.Collections.Generic;
using hr_crm.Entities;

namespace hr_crm.DTO
{
    public class OffBoardingDto
    {

            public int UserId { get; set; }
            public DateTime ResignationDate { get; set; }
            public DateTime LastWorkingDate { get; set; }
            public string Reason { get; set; }
            public bool AccountDeactivation { get; set; }
    }

        public class UpdateOffboardingStatusDTO
        {
            public string KnowledgeTransferStatus { get; set; }
            public string AssetReturnStatus { get; set; }
            public string ExitInterviewStatus { get; set; }
            public string OverallStatus { get; set; }
        }
    

}


