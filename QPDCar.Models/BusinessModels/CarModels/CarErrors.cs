namespace QPDCar.Models.BusinessModels.CarModels;

/// <summary> Типы ошибок связанные с машиной </summary>
public enum CarErrors
{
    CarNotFound,
    CarNotSaved,
    CarNotUpdated,
    CarNotDeleted,
    
    CarNotSetPhoto,
    CarAlreadyHavePhoto,
    
    CarIsSold,
}

/// <summary> Типы ошибок связанные с корзиной </summary>
public enum CartErrors
{
    NoOneCarInCart
}