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
            var result =
                await _service
                    .ReviewDocumentAsync(id, dto);

            if (!result)
                return NotFound("Document not found");

            return Ok("Document reviewed successfully");
        }
    }
}
