using Minio;
using Minio.DataModel.Args;
using MinioTest.Model;

namespace MinioTest.Services.Minio
{
    public class MinioManager : IMinioService
    {
        private readonly IMinioClient _minioClient;
        //private readonly string? _bucket = Accessor.AppConfiguration["Minio:Bucket"];
     
        private readonly string? _bucket = "test-bucket";

        private Task<bool> IsBucketExists() =>
            _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucket));

        private async Task<string> IsFileExists(string token)
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucket)
                .WithObject(token);

            var status = await _minioClient.StatObjectAsync(statObjectArgs);
            if (status == null)
                throw new Exception("File not found or deleted");

            return status.ContentType;
        }

        public MinioManager(IMinioClientFactory minioClientFactory)
        {
            _minioClient = minioClientFactory.CreateClient();
        }

        public async Task<string> PutObject(IFormFile file)
        {
            if (!await IsBucketExists())
                throw new Exception("NotFound");

            var filestream = new MemoryStream(await file.GetBytes());
            var filename = Guid.NewGuid().ToString();

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_bucket)
                .WithObject(filename)
                .WithStreamData(filestream)
                .WithObjectSize(filestream.Length)
                .WithContentType(file.ContentType);

            await _minioClient.PutObjectAsync(putObjectArgs);
            return filename;
        }

        public async Task<GetObjectDto> GetObject(string token)
        {
            if (!await IsBucketExists())
                throw new Exception("NotFound");

            var contentType = await IsFileExists(token);

            var destination = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(token)
                .WithCallbackStream((stream) => { stream.CopyTo(destination); });
            await _minioClient.GetObjectAsync(getObjectArgs);

            return new GetObjectDto()
            {
                Bytes = destination.ToArray(),
                ContentType = contentType
            };
        }
    }

    public static class FormFileExtensions
    {
        public static async Task<byte[]> GetBytes(this IFormFile formFile)
        {
            await using var memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}