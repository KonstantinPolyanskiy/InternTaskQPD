namespace QPDCar.Models.ApplicationModels.Settings;

/// <summary> Настройки подклчюения к Minio </summary>
public class MinioSettings
{
    public string Endpoint { get; set; } = string.Empty;
    public int Port { get; set; }
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSSL { get; set; }
    public string BucketName { get; set; } = string.Empty;
}