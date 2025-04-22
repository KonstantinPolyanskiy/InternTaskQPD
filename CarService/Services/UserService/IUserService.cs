using CarService.Models.User;
using CarService.Models.User.Dtos;

namespace CarService.Services.UserService;

public interface IUserService
{
    Task<IUser> CreateUserAsync(RegistrationRequestDto registrationUserDto);
}