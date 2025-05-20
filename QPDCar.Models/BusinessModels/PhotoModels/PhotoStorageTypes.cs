namespace QPDCar.Models.BusinessModels.PhotoModels;

/// <summary> Где хранить данные фотографии машины </summary>
public enum PhotoStorageTypes
{
    None,
    Database,
    Minio,
    FileSystem
}