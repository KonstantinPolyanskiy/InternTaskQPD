namespace Public.UseCase.Models.CarModels;

public record CarsUseCaseResponse
{
    public CarUseCaseResponse[] Cars { get; set; } = null!;
    
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

