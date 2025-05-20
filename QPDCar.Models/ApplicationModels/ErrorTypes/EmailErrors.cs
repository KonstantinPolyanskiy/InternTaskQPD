namespace QPDCar.Models.ApplicationModels.ErrorTypes;

/// <summary> Типы ошибок связанные с почтой </summary>
public enum EmailErrors
{
    MailNotSend, 
}

/// <summary> Типы ошибок связанные с подтверждением почты </summary>
public enum EmailTokenErrors
{
    IncorrectUserOrExpired
}