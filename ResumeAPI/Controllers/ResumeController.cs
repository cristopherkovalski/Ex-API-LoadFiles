using Microsoft.AspNetCore.Mvc;

namespace ResumeAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResumeController : ControllerBase
    {
        private readonly string _uploadsFolder;

        public ResumeController()
        {
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(_uploadsFolder))
                Directory.CreateDirectory(_uploadsFolder);
        }

        [HttpPost]
        [Route("upload")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest("Nenhum arquivo enviado.");

                
                if (file.Length > 5 * 1024 * 1024) 
                    return BadRequest("O tamanho do arquivo excede 5MB.");

               
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (Array.IndexOf(allowedExtensions, extension) == -1)
                    return BadRequest("Formato de arquivo inválido. Permitido: PDF, DOC, DOCX.");

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(_uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Ok("Currículo enviado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao enviar currículo: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("download/{fileName}")]
        public IActionResult DownloadResume(string fileName)
        {
            var filePath = Path.Combine(_uploadsFolder, fileName);

            if (!System.IO.File.Exists(filePath))
                return NotFound("Currículo não encontrado.");

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            var contentType = "application/octet-stream";
            return File(memory, contentType, Path.GetFileName(filePath));
        }
    }
}
