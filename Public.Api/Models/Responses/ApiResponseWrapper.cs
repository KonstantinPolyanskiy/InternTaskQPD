using Public.Models.CommonModels;

namespace Public.Api.Models.Responses;

public sealed class ApiResponseWrapper<T>
{
    public T? Data { get; set; }
    
    public List<ApplicationError>? Errors { get; set; }
    public List<ApplicationError>? Warnings { get; set; }
}