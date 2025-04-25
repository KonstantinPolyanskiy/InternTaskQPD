namespace Models.Bridge.Car;

/// <summary>Состояние авто</summary>
public enum Condition
{
    Unknown = 0,
    New = 1,
    Used = 2,
    NotWorking = 3,
}

/// <summary>Приоритет продажи</summary>
public enum Priority
{
    Unknown = 0,
    Low = 1,
    Medium = 2,
    High = 3,
}
