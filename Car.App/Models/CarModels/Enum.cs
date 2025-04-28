namespace Car.App.Models.CarModels;

/// <summary> Приоритетное хранилище для фото </summary>
public enum PhotoStorageType
{
    Database = 1,
    FileStorage = 2,
    Minio = 3
}

/// <summary>Состояние авто</summary>
public enum CarCondition
{
    Unknown = 0,
    New = 1,
    Used = 2,
    NotWorking = 3,
}

/// <summary>Приоритет продажи</summary>
public enum CarPrioritySale
{
    Unknown = 0,
    Low = 1,
    Medium = 2,
    High = 3,
}