using MinioTest.Model;

namespace MinioTest.Services.Minio;

public interface IMinioService
{
    Task<string> PutObject(IFormFile file);
    Task<GetObjectDto> GetObject(string token);
}