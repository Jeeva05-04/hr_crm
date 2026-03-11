using hr_crm.Entities;

namespace hr_crm.Repositories.Interface
{
    public interface IDigitalSignatureRepository
    {

            Task<DigitalSignature> CreateRequestAsync(DigitalSignature signature);
            Task<DigitalSignature?> GetByIdAsync(int signatureId);
            Task<DigitalSignature> UpdateAsync(DigitalSignature signature);
            Task<IEnumerable<DigitalSignature>> GetByUserIdAsync(int userId);
            Task<IEnumerable<DigitalSignature>> GetAllAsync();
            Task DeleteAsync(DigitalSignature signature);

    }
}

