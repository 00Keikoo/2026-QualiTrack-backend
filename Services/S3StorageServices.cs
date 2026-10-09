using Amazon.S3;
using Amazon.S3.Model;

namespace QualiTrack.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly string _endpoint;

    public S3StorageService(IConfiguration config)
    {
        _endpoint = config["AWS_ENDPOINT_URL"]
            ?? Environment.GetEnvironmentVariable("AWS_ENDPOINT_URL")
            ?? config["S3:Endpoint"]
            ?? throw new InvalidOperationException("S3 endpoint tidak ditemukan");

        _bucketName = config["AWS_S3_BUCKET_NAME"]
            ?? Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME")
            ?? config["S3:BucketName"]
            ?? throw new InvalidOperationException("S3 bucket name tidak ditemukan");

        var accessKey = config["AWS_ACCESS_KEY_ID"]
            ?? Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")
            ?? config["S3:AccessKeyId"]
            ?? throw new InvalidOperationException("S3 access key tidak ditemukan");

        var secretKey = config["AWS_SECRET_ACCESS_KEY"]
            ?? Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
            ?? config["S3:SecretAccessKey"]
            ?? throw new InvalidOperationException("S3 secret key tidak ditemukan");

        _s3 = new AmazonS3Client(
            accessKey,
            secretKey,
            new AmazonS3Config
            {
                ServiceURL = _endpoint,
                ForcePathStyle = true
            }
        );
    }

    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var key = $"uploads/{fileName}";

        using var stream = file.OpenReadStream();
        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType
        });

        return key;
    }

    public string GetPresignedUrl(string key, int expiryHours = 24)
    {
        return _s3.GetPreSignedURL(new GetPreSignedUrlRequest 
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Protocol = Protocol.HTTPS
        });
    }

    public async Task DeleteFileAsync(string key)
    {
        await _s3.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        });
    }
}