using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;

namespace QPDCar.Api.Models.Responses;

/// <summary> Оберта для ответа приложения, с самими бизнес данными и ошибками/предупреждениями </summary>
public sealed class ApiResponseWrapper<T>
{
    public T? Data { get; set; }
    
    public List<ApplicationError>? Errors { get; set; }
    public List<ApplicationError>? Warnings { get; set; }
}