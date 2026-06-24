using Document_Management_System.DTOs;
using Document_Management_System.Interfaces;
using Document_Management_System.Models;
using Document_Management_System.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Document_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]

    public class DocumentsController : ControllerBase
    {
        private readonly IDocumentService _service;

        public DocumentsController(IDocumentService service)
        {
            _service = service;
        }

        //Candidate Upload Endpoint
        [HttpPost("upload")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> Upload(
           [FromForm] UploadDocumentRequestDTO request)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.UploadAsync(request, userId);

            return Ok(result);
        }

        // Candidate View Own Documents
        [HttpGet("my-documents")]
        [Authorize(Roles = "Candidate")]
        public async Task<IActionResult> MyDocuments()
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _service.GetMyDocumentsAsync(userId);

            return Ok(result);
        }

        // Admin View All Documents
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllDocumentsAsync();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("review/{id}")]
        public async Task<IActionResult> ReviewDocument(
            int id,
            ReviewDocumentDTO dto)
        {
            var adminId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result =
                await _service
                    .ReviewDocumentAsync(id, dto, adminId);

            if (!result)
                return NotFound("Document not found");

            return Ok("Document reviewed successfully");
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(int id)
        {
            var userId = int.Parse(
                User.FindFirst(
                    ClaimTypes.NameIdentifier)!.Value);

            var role = User.FindFirst(
                ClaimTypes.Role)!.Value;

            var result =
                await _service.DownloadAsync(
                    id,
                    userId,
                    role);

            return File(
                result.FileBytes,
                result.ContentType,
                result.FileName);
        }

        [HttpPost("search")]
        public async Task<IActionResult> Search(
        SearchDocumentDTO request)
        {
            var userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var role = User.FindFirst(ClaimTypes.Role)!.Value;

            var result = await _service.SearchAsync(
                request,
                userId,
                role);

            return Ok(result);
        }
    }
}
