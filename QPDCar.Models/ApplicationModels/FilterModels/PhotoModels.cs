namespace QPDCar.Models.ApplicationModels.FilterModels;

/// <summary> Фильтрация для фото при запросе машин (с фото, без, без разницы) </summary>
public enum HavePhotoTermination
{
    WithPhoto,
    WithoutPhoto,
    NoMatter
}