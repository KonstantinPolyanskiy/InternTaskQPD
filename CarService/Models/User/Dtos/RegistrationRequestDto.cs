using Newtonsoft.Json;

namespace CarService.Models.User.Dtos;

public class RegistrationRequestDto
{
    [JsonProperty("username")]
    public string Username { get; set; }
    
    [JsonProperty("password")]
    public string Password { get; set; }
    
    [JsonProperty("email")]
    public string Email { get; set; }
}