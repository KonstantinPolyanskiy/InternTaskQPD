namespace Car.Api.Profiles.Extensions;

public static class StreamExtension
{
    /// <summary>
    /// Расширение <see cref="Stream"/> для преобразования потока с фото в base64 строку
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public static async Task<string> ToBase64StringAsync(this Stream stream, CancellationToken ct = default)
    {
        await using (stream)
        {
            
        }
    }
}