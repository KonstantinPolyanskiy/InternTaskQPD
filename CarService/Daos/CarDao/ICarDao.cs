using System.Xml.Linq;
using CarService.Models.Car;

namespace CarService.Daos.CarDao;

public interface ICarDao
{
    Task<int?> SaveAsync(ICar car);
    Task<bool?> DeleteAsync(int id);
    Task<ICar?> GetByIdAsync(int id);
    
    Task<ICar?> UpdateAsync(int id, ICar newCar);
    
    Task<List<ICar>> GetAllCarsAsync();
}