using System.Security.Cryptography;
using System.Text;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;

namespace hr_crm.Service
{
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly IDigitalSignatureRepository _repo;
        private readonly string _uploadPath;

        public DigitalSignatureService(IDigitalSignatureRepository repo)
        {
            _repo = repo;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        public async Task<DigitalSignatureResponseCreateDto> RequestSignatureAsync(DigitalSignatureRequestCreateDto dto)
        {
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.DocumentFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.DocumentFile.CopyToAsync(stream);
            }

            var entity = new DigitalSignature
            {
                UserId = dto.UserId,
                RequestedBy = dto.RequestedBy,
                DocumentName = dto.DocumentName,
                DocumentType = dto.DocumentType,
                Status = "Pending",
                RequestedAt = DateTime.UtcNow,
                Remarks = dto.Remarks
            };

            var created = await _repo.CreateRequestAsync(entity);
            return MapToResponse(created);
        }

        public async Task<DigitalSignatureResponseCreateDto> SignDocumentAsync(int signatureId, DigitalSignatureSignCreateDto dto)
        {
            var entity = await _repo.GetByIdAsync(signatureId)
                ?? throw new Exception($"Signature request {signatureId} not found.");

            if (entity.UserId != dto.UserId)
                throw new UnauthorizedAccessException("You are not authorized to sign this document.");

            if (entity.Status == "Signed")
                throw new InvalidOperationException("Document is already signed.");

            var rawData = $"{signatureId}-{dto.UserId}-{DateTime.UtcNow}";
            entity.SignatureHash = GenerateHash(rawData);
            entity.Status = "Signed";
            entity.SignedAt = DateTime.UtcNow;
            entity.SignedByIp = dto.SignedByIp;
            entity.Remarks = dto.Remarks ?? entity.Remarks;

            var updated = await _repo.UpdateAsync(entity);
            return MapToResponse(updated);
        }

        public async Task<DigitalSignatureResponseCreateDto?> GetStatusAsync(int signatureId)
        {
            var entity = await _repo.GetByIdAsync(signatureId);
            return entity is null ? null : MapToResponse(entity);
        }

        public async Task<IEnumerable<DigitalSignatureResponseCreateDto>> GetHistoryAsync(int userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);
            return list.Select(MapToResponse);
        }

        public async Task<DigitalSignatureResponseCreateDto> UpdateRequestAsync(int signatureId, DigitalSignatureRequestCreateDto dto)
        {
            var entity = await _repo.GetByIdAsync(signatureId)
                ?? throw new Exception($"Signature request {signatureId} not found.");

            if (entity.Status == "Signed")
                throw new InvalidOperationException("Signed document cannot be updated.");

            entity.UserId = dto.UserId;
            entity.RequestedBy = dto.RequestedBy;
            entity.DocumentName = dto.DocumentName;
            entity.DocumentType = dto.DocumentType;
            entity.Remarks = dto.Remarks;

            var updated = await _repo.UpdateAsync(entity);
            return MapToResponse(updated);
        }

        public async Task<IEnumerable<DigitalSignature>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<bool> DeleteRequestAsync(int signatureId)
        {
            var entity = await _repo.GetByIdAsync(signatureId)
                ?? throw new Exception($"Signature request {signatureId} not found.");

            if (entity.Status == "Signed")
                throw new InvalidOperationException("Signed document cannot be deleted.");

            await _repo.DeleteAsync(entity);
            return true;
        }

        private static string GenerateHash(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static DigitalSignatureResponseCreateDto MapToResponse(DigitalSignature e)
        {
            return new DigitalSignatureResponseCreateDto
            {
                SignatureId = e.SignatureId,
                UserId = e.UserId,
                RequestedBy = e.RequestedBy,
                DocumentName = e.DocumentName,
                DocumentType = e.DocumentType,
                Status = e.Status,
                SignatureHash = e.SignatureHash,
                SignedByIp = e.SignedByIp,
                RequestedAt = e.RequestedAt,
                SignedAt = e.SignedAt,
                Remarks = e.Remarks
            };
        }
    }
}