using Microsoft.AspNetCore.Mvc;
using MinioTest.Services.Minio;

namespace MinioTest.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ObjectController : ControllerBase
    {
        private readonly ILogger<ObjectController> _logger;
        private readonly IMinioService _minioService;

        public ObjectController(ILogger<ObjectController> logger, IMinioService minioService)
        {
            _logger = logger;
            _minioService = minioService;
        }
        [HttpGet]
        public async Task<ActionResult> Get(string token)
        {
            var result = await _minioService.GetObject(token);
            return File(result.Bytes, result.ContentType);
        }

        [HttpPost]
        public async Task<ActionResult> Post(IFormFile file)
        {
            var result = await _minioService.PutObject(file);
            return Ok(new { filename = result });
        }
    }
}