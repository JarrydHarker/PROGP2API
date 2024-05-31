using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace FirebaseImageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FirebaseService _firebaseService;

        public FilesController(FirebaseService firebaseService)
        {
            _firebaseService = firebaseService;
        }

        [HttpPost("Upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var localPath = Path.GetTempFileName();
            using (var stream = new FileStream(localPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            string remotePath = "Users/"+ file.FileName;
            var objectName = file.FileName; // Use a unique name for production
            var mediaLink = await _firebaseService.UploadImageAsync(localPath, remotePath);

            return Ok(new { Url = mediaLink });
        }

        /*[HttpGet("download/{objectName}")]
        public async Task<IActionResult> DownloadFile(string objectName)
        {
            var localPath = Path.GetTempFileName();
            await _firebaseService.DownloadImageAsync(localPath);

            var memory = new MemoryStream();
            using (var stream = new FileStream(localPath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", objectName);
        }*/

        [HttpGet("Image/{filePath}")]
        public async Task<IActionResult> GetFileDownloadUrl(string filePath)
        {

            // Decode the URL-encoded file path
            string decodedFilePath = Uri.UnescapeDataString(filePath);
            try
            {
                var url = await _firebaseService.GetFileUrlAsync(decodedFilePath);
                return Ok(new { Url = url });
            }
            catch (Google.GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(new { error = $"File not found: {decodedFilePath}" });
            }
        }

        [HttpGet("Url/{filePath}")]
        public async Task<string> GetPublicFileUrl(string filePath)
        {

            // Decode the URL-encoded file path
            string decodedFilePath = Uri.UnescapeDataString(filePath);
            try
            {
                var url = await _firebaseService.GeneratePublicUrl(decodedFilePath);
                return url.ToString();
            }
            catch (Google.GoogleApiException e) when (e.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return e.ToString();
            }


        }
    }
}
