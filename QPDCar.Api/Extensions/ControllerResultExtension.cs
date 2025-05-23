﻿using Microsoft.AspNetCore.Mvc;
using QPDCar.Api.Models.Responses;
using QPDCar.Models.ApplicationModels;
using QPDCar.Models.ApplicationModels.ApplicationResult;

namespace QPDCar.Api.Extensions;

public static class ControllerResultExtension
{
    public static IActionResult ToApiResult<T>(this ControllerBase controller,
        ApplicationExecuteResult<T> logicResult)
    {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(logicResult);

        if (logicResult.IsSuccess is not true)
        {
            var errors   = logicResult.GetCriticalErrors;
            var warnings = logicResult.GetWarnings;

            // Если в ошибке прописан HttpStatusCode – используем его; иначе 500.
            if (errors.Any())
            {
                var errorBody = new ApiResponseWrapper<object>
                {
                    Data     = null,
                    Errors   = errors.ToList(),
                    Warnings = warnings.Any() ? warnings.ToList() : null
                };
                
                var err = errors.FirstOrDefault()!;
                var code = 500;
                
                if (err.HttpStatusCode is not null)
                    code = (int)err.HttpStatusCode.Value;
                
                return controller.StatusCode(code , errorBody);
            }
        }

        var successBody = new ApiResponseWrapper<T>
        {
            Data     = logicResult.Value,
            Errors   = null,
            Warnings = logicResult.GetWarnings.Any() ? logicResult.GetWarnings as List<ApplicationError> : null
        };

        return controller.Ok(successBody);
    }
}