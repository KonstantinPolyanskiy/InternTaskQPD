using QPDCar.Models.ApplicationModels;

namespace QPDCar.Api.Models.Responses;

public sealed class ApiResponseWrapper<T>
{
    public T? Data { get; set; }
    
    public List<ApplicationError>? Errors { get; set; }
    public List<ApplicationError>? Warnings { get; set; }
}