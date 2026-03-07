using AutoMapper;
using hr_crm.Entities;
using hr_crm.DTO;

namespace hr_crm.Mappings
{
    public class OnboardingMappingProfile : Profile
    {
        public OnboardingMappingProfile()
        {
            // MAIN ENTITY
            CreateMap<EmployeeOnboardingCreateDto, EmployeeOnboarding>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Submitted"))
                .ForMember(dest => dest.LaptopImagePath, opt => opt.Ignore());

            // WORK EXPERIENCE
            CreateMap<EmployeeOnboardingCreateDto, WorkExperience>()
                .ForMember(dest => dest.EmployeeOnboardingId, opt => opt.Ignore())
                .ForMember(dest => dest.PreviousCompanyPayslipPath, opt => opt.Ignore());

            // DOCUMENTS
            CreateMap<EmployeeOnboardingCreateDto, EmployeeOnboardingDocuments>()
                .ForMember(dest => dest.EmployeeOnboardingId, opt => opt.Ignore())
                .ForMember(dest => dest.AadharCardPath, opt => opt.Ignore())
                .ForMember(dest => dest.PANCardPath, opt => opt.Ignore())
                .ForMember(dest => dest.BankStatementPath, opt => opt.Ignore())
                .ForMember(dest => dest.BankPassbookPath, opt => opt.Ignore())
                .ForMember(dest => dest.ParentAadharPath, opt => opt.Ignore())   // FIXED
                .ForMember(dest => dest.ExperienceLetterPath, opt => opt.Ignore())
                .ForMember(dest => dest.AcceptanceLetterPath, opt => opt.Ignore())
                .ForMember(dest => dest.HighestQualificationDocumentPath, opt => opt.Ignore());
        }
    }
}