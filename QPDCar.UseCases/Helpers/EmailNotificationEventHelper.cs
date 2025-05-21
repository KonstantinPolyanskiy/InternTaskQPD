using QPDCar.Models.ApplicationModels.Events;

namespace QPDCar.UseCases.Helpers;

internal static class EmailNotificationEventHelper
{
    internal static EmailNotificationEvent BuildConfirmEmailEvent(string to, Uri uri)
        => new()
        {
            MessageId = Guid.NewGuid(),
            To = to,
            Subject = "Подтвердите почту на сайте CarService.site",
            BodyHtml = $"Перейдите по {uri.ToString()} для подтверждения почты"
        };

    internal static EmailNotificationEvent BuildCarWithoutPhotoEmailEvent(string to, int carId)
        => new()
        {
            MessageId = Guid.NewGuid(),
            To = to,
            Subject = $"Машина {carId} без фото",
            BodyHtml = $"Вы добавили машину с id {carId} без фотографии, пожалуйста, добавьте ее позже"
        };

    internal static EmailNotificationEvent BuildAccountLoginEvent(string to, string loginTime, string login)
        => new()
        {
            MessageId = Guid.NewGuid(),
            To = to,
            Subject = $"Вход в аккаунт {login}",
            BodyHtml = $"В ваш аккаунт {login} был совершен вход {loginTime}"
        };

    internal static EmailNotificationEvent BuildThanksEmailConfirmEvent(string to, string login)
        => new()
        {
            MessageId = Guid.NewGuid(),
            To = to,
            Subject = $"Спасибо за подтверждение почты",
            BodyHtml = $"Спасибо за подтверждение почты аккаунта {login}"
        };
}