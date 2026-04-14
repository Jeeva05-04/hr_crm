using System.Security.Cryptography;
using System.Text;
using hr_crm.DTO;
using hr_crm.Entities;
using hr_crm.Repositories.Interface;
using hr_crm.Service.Interface;
using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace hr_crm.Service
{
    public class DigitalSignatureService : IDigitalSignatureService
    {
        private readonly IDigitalSignatureRepository _repo;
        private readonly NotificationService _notification;
        private readonly string _uploadPath;

        public DigitalSignatureService(IDigitalSignatureRepository repo, NotificationService notification)
        {
            _repo = repo;
            _notification = notification;
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "signatures");

            if (!Directory.Exists(_uploadPath))
                Directory.CreateDirectory(_uploadPath);
        }

        // HR uploads a document and sends it to the employee for signing
        public async Task<DigitalSignatureResponseCreateDto> RequestSignatureAsync(DigitalSignatureRequestCreateDto dto)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.DocumentFile.FileName);
            var filePath = Path.Combine(_uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.DocumentFile.CopyToAsync(stream);
            }

            var relativeFilePath = $"/uploads/signatures/{fileName}";

            var entity = new DigitalSignature
            {
                UserId = dto.UserId,
                RequestedBy = dto.RequestedBy,
                DocumentName = dto.DocumentName,
                DocumentType = dto.DocumentType,
                Status = "Pending",
                FilePath = relativeFilePath,
                RequestedAt = DateTime.UtcNow,
                Remarks = dto.Remarks
            };

            var created = await _repo.CreateRequestAsync(entity);

            // Notify employee that HR has sent a document to sign
            await _notification.CreateNotification(
                userId: dto.UserId,
                title: "New Document to Sign",
                message: $"HR has sent you a document: \"{dto.DocumentName}\" ({dto.DocumentType}). Please review and sign it.",
                module: "DigitalSignature",
                referenceId: created.SignatureId
            );

            return MapToResponse(created);
        }

        // Employee signs the document — embeds signature image into PDF and stores signed copy
        public async Task<DigitalSignatureResponseCreateDto> SignDocumentAsync(int signatureId, DigitalSignatureSignCreateDto dto)
        {
            var entity = await _repo.GetByIdAsync(signatureId)
                ?? throw new Exception($"Signature request {signatureId} not found.");

            if (entity.UserId != dto.UserId)
                throw new UnauthorizedAccessException("You are not authorized to sign this document.");

            if (entity.Status == "Signed")
                throw new InvalidOperationException("Document is already signed.");

            if (string.IsNullOrEmpty(entity.FilePath))
                throw new InvalidOperationException("Original document file not found.");

            // Generate signed PDF with embedded signature
            var signedFileName = $"signed_{Guid.NewGuid()}.pdf";
            var signedAbsolutePath = Path.Combine(_uploadPath, signedFileName);
            var originalAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            EmbedSignatureIntoPdf(
                originalAbsolutePath,
                signedAbsolutePath,
                dto.SignatureImageBase64,
                signerUserId: dto.UserId,
                signerIp: dto.SignedByIp,
                documentName: entity.DocumentName
            );

            var rawData = $"{signatureId}-{dto.UserId}-{DateTime.UtcNow:O}";
            entity.SignatureHash = GenerateHash(rawData);
            entity.Status = "Signed";
            entity.SignedAt = DateTime.UtcNow;
            entity.SignedByIp = dto.SignedByIp;
            entity.SignedFilePath = $"/uploads/signatures/{signedFileName}";
            entity.Remarks = dto.Remarks ?? entity.Remarks;

            var updated = await _repo.UpdateAsync(entity);

            // Notify HR (RequestedBy) that employee has signed
            await _notification.CreateNotification(
                userId: entity.RequestedBy,
                title: "Document Signed",
                message: $"Employee (ID: {dto.UserId}) has signed the document: \"{entity.DocumentName}\". You can now download the signed copy.",
                module: "DigitalSignature",
                referenceId: signatureId
            );

            return MapToResponse(updated);
        }

        // HR views the original uploaded document
        public async Task<(byte[] FileBytes, string FileName)?> ViewDocumentAsync(int signatureId)
        {
            var entity = await _repo.GetByIdAsync(signatureId);
            if (entity is null || string.IsNullOrEmpty(entity.FilePath))
                return null;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                return null;

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return (bytes, $"{entity.DocumentName}.pdf");
        }

        // HR downloads the signed document after employee has signed
        public async Task<(byte[] FileBytes, string FileName)?> DownloadSignedDocumentAsync(int signatureId)
        {
            var entity = await _repo.GetByIdAsync(signatureId);
            if (entity is null || string.IsNullOrEmpty(entity.SignedFilePath))
                return null;

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.SignedFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
                return null;

            var bytes = await File.ReadAllBytesAsync(fullPath);
            return (bytes, $"{entity.DocumentName}_Signed.pdf");
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

            if (!string.IsNullOrEmpty(entity.FilePath))
            {
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", entity.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }

            await _repo.DeleteAsync(entity);
            return true;
        }

        // Embeds the employee's signature image and a text stamp onto the last page of the PDF
        private static void EmbedSignatureIntoPdf(string originalPath, string signedPath, string? signatureImageBase64, int signerUserId, string? signerIp, string documentName)
        {
            using var reader = new PdfReader(originalPath);
            using var writer = new PdfWriter(signedPath);
            using var pdfDoc = new PdfDocument(reader, writer);
            using var document = new Document(pdfDoc);

            var pageCount = pdfDoc.GetNumberOfPages();
            var lastPage = pdfDoc.GetPage(pageCount);
            var pageSize = lastPage.GetPageSize();
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            float signatureX = pageSize.GetWidth() - 230;
            float signatureY = 80;

            // Embed drawn signature image if provided
            if (!string.IsNullOrEmpty(signatureImageBase64))
            {
                var base64Data = signatureImageBase64.Contains(',')
                    ? signatureImageBase64.Split(',')[1]
                    : signatureImageBase64;

                var imgBytes = Convert.FromBase64String(base64Data);
                var imgData = ImageDataFactory.Create(imgBytes);
                var img = new Image(imgData)
                    .ScaleToFit(160, 65)
                    .SetFixedPosition(pageCount, signatureX, signatureY + 20);
                document.Add(img);
            }

            // Add text stamp: signer info + timestamp
            var stampText = $"Signed by User ID: {signerUserId}\nDate: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC\nIP: {signerIp ?? "N/A"}";
            var stampPara = new Paragraph(stampText)
                .SetFont(font)
                .SetFontSize(7)
                .SetFixedPosition(pageCount, signatureX, signatureY - 25, 200);
            document.Add(stampPara);
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
                FilePath = e.FilePath,
                SignedFilePath = e.SignedFilePath,
                SignatureHash = e.SignatureHash,
                SignedByIp = e.SignedByIp,
                RequestedAt = e.RequestedAt,
                SignedAt = e.SignedAt,
                Remarks = e.Remarks
            };
        }
    }
}
