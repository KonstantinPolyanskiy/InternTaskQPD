﻿namespace QPDCar.Models.ApplicationModels.Settings;

/// <summary> Настройки JWT токенов </summary>
public class JwtAuthSettings
{
    public string Issuer { get; set; }                = null!;
    public string Audience { get; set; }              = null!;
    
    public string SecretKey { get; set; }             = null!;
    
    public int AccessTokenLifetimeMinutes { get; set; }
    public int RefreshTokenLifetimeDays    { get; set; }
}