namespace QPDCar.UseCases.Models.UserModels;

public class EmployerUseCaseResponse
{
    public Guid? Id { get; set; }
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    public string? Login { get; set; }
}