using System.Security.Cryptography;
using System.Text;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories;
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
            // Sets the storage path to wwwroot/uploads/signatures
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");
            if (!Directory.Exists(_uploadPath)) Directory.CreateDirectory(_uploadPath);
        }
        public async Task<DigitalSignatureResponseCreateDto> RequestSignatureAsync(DigitalSignatureRequestCreateDto dto)
        {
            // 1. Path where files will be stored
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            // 2. Process the file from the DTO (The one you picked with 'Choose File')
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.DocumentFile.FileName);
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.DocumentFile.CopyToAsync(stream);
            }

            // 3. Create the database record
            var entity = new DigitalSignature
            {
                EmployeeId = dto.EmployeeId,
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

        // ── 2. Employee signs the document ───────────────────────────────────
        public async Task<DigitalSignatureResponseCreateDto> SignDocumentAsync(int signatureId, DigitalSignatureSignCreateDto dto)
            {
                var entity = await _repo.GetByIdAsync(signatureId)
                    ?? throw new Exception($"Signature request {signatureId} not found.");

                if (entity.EmployeeId != dto.EmployeeId)
                    throw new UnauthorizedAccessException("You are not authorized to sign this document.");

                if (entity.Status == "Signed")
                    throw new InvalidOperationException("Document is already signed.");

                // Generate a unique hash for the signature
                var rawData = $"{signatureId}-{dto.EmployeeId}-{DateTime.UtcNow}";
                entity.SignatureHash = GenerateHash(rawData);
                entity.Status = "Signed";
                entity.SignedAt = DateTime.UtcNow;
                entity.SignedByIp = dto.SignedByIp;
                entity.Remarks = dto.Remarks ?? entity.Remarks;

                var updated = await _repo.UpdateAsync(entity);
                return MapToResponse(updated);
            }

            // ── 3. Get status of a signature request ─────────────────────────────
            public async Task<DigitalSignatureResponseCreateDto?> GetStatusAsync(int signatureId)
            {
                var entity = await _repo.GetByIdAsync(signatureId);
                return entity is null ? null : MapToResponse(entity);
            }

            // ── 4. Get all signatures by employee ────────────────────────────────
            public async Task<IEnumerable<DigitalSignatureResponseCreateDto>> GetHistoryAsync(int employeeId)
            {
                var list = await _repo.GetByEmployeeIdAsync(employeeId);
                return list.Select(MapToResponse);
            }

            // ── Helpers ──────────────────────────────────────────────────────────
            private static string GenerateHash(string input)
            {
                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(bytes).ToLower();
            }

            private static DigitalSignatureResponseCreateDto MapToResponse(DigitalSignature e) => new()
            {
                SignatureId = e.SignatureId,
                EmployeeId = e.EmployeeId,
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
        public async Task<DigitalSignatureResponseCreateDto> UpdateRequestAsync(int signatureId, DigitalSignatureRequestCreateDto dto)
        {
            var entity = await _repo.GetByIdAsync(signatureId)
                ?? throw new Exception($"Signature request {signatureId} not found.");

            if (entity.Status == "Signed")
                throw new InvalidOperationException("Signed document cannot be updated.");

            // Update fields
            entity.DocumentName = dto.DocumentName;
            entity.DocumentType = dto.DocumentType;
            entity.Remarks = dto.Remarks;
            entity.EmployeeId = dto.EmployeeId;
            entity.RequestedBy = dto.RequestedBy;

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

    }
}

