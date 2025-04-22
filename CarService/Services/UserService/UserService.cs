using AutoMapper;
using CarService.Models.User;
using CarService.Models.User.Dtos;

namespace CarService.Services.UserService;

public class UserService(IUserDao userDao, IMapper mapper) : IUserService
{
    public Task<IUser> CreateUserAsync(RegistrationRequestDto registrationUserDto)
    {
        throw new NotImplementedException();
    }
}