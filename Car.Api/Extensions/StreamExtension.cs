namespace CarService.Extensions;

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
            if (stream is MemoryStream ms && ms.TryGetBuffer(out var buffer))
                return Convert.ToBase64String(buffer);
            
            await using var copy = new MemoryStream();
            
            await stream.CopyToAsync(copy, ct);
            return Convert.ToBase64String(copy.GetBuffer(), 0, (int)copy.Length);
        }
    }
}