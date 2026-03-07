using System.Collections.Generic;
using System.Threading.Tasks;
using hr_crm.DTO;
using hr_crm.Entities;

namespace hr_crm.Service.Interface
{
    public interface IDigitalSignatureService
    {
        Task<DigitalSignatureResponseCreateDto> RequestSignatureAsync(DigitalSignatureRequestCreateDto dto);
        Task<DigitalSignatureResponseCreateDto> SignDocumentAsync(int signatureId, DigitalSignatureSignCreateDto dto);
        Task<DigitalSignatureResponseCreateDto?> GetStatusAsync(int signatureId);
        Task<IEnumerable<DigitalSignatureResponseCreateDto>> GetHistoryAsync(int employeeId);
        Task<DigitalSignatureResponseCreateDto> UpdateRequestAsync(int signatureId, DigitalSignatureRequestCreateDto dto);
        Task<IEnumerable<DigitalSignature>> GetAllAsync();
        Task<bool> DeleteRequestAsync(int signatureId);
    }
}