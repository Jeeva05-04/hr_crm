using AutoMapper;
using hr_crm.Entities;
using hr_crm.DTO;

namespace hr_crm.Mappings
{
    public class OnboardingMappingProfile : Profile
    {
        public OnboardingMappingProfile()
        {
            CreateMap<EmployeeOnboardingDto, EmployeeOnboarding>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Submitted"))
                .ForMember(dest => dest.LaptopImagePath, opt => opt.Ignore());

            CreateMap<EmployeeOnboardingDto, EmployeeOnboardingWorkExperience>()
                .ForMember(dest => dest.EmployeeOnboardingId, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousCompanyPayslipPath, opt => opt.Ignore());

            CreateMap<EmployeeOnboardingDto, EmployeeOnboardingDocuments>()
            .ForMember(dest => dest.EmployeeOnboardingId, opt => opt.Ignore())
            .ForMember(dest => dest.AadharCardPath, opt => opt.Ignore())
            .ForMember(dest => dest.PANCardPath, opt => opt.Ignore())
            .ForMember(dest => dest.BankStatementPath, opt => opt.Ignore())
            .ForMember(dest => dest.BankPassbookPath, opt => opt.Ignore())
            .ForMember(dest => dest.ParentAadharPaths, opt => opt.Ignore()) // ✅ updated
            .ForMember(dest => dest.ExperienceLetterPath, opt => opt.Ignore()) // ✅ new
            .ForMember(dest => dest.AcceptanceLetterPath, opt => opt.Ignore()) // ✅ new
            .ForMember(dest => dest.HighestQualificationDocumentPath, opt => opt.Ignore());
        }
    }
}