using APITesting.Common.Constants;
using APITesting.Common.Localization;
using APITesting.Common.Responses;
using Microsoft.AspNetCore.Mvc;

namespace APITesting.Common.Extensions;

public static class ControllerExtensions
{
    public static IServiceCollection AddApplicationControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var languageProvider = context.HttpContext.RequestServices
                        .GetRequiredService<LanguageProvider>();

                    var language = context.HttpContext.Request.Headers[GlobalParams.Headers.Language]
                        .FirstOrDefault() ?? GlobalParams.App.DefaultLanguage;

                    var errors = context.ModelState
                        .Where(item => item.Value?.Errors.Count > 0)
                        .SelectMany(item => item.Value!.Errors.Select(error => new ValidationErrorItem
                        {
                            Field = item.Key,
                            Message = error.ErrorMessage
                        }))
                        .ToList();

                    var response = new ApiResponse(
                        status: languageProvider.Get(language, GlobalParams.Lang.Status.Failed),
                        message: languageProvider.Get(language, GlobalParams.Lang.Message.ValidationFailed),
                        errors: errors);

                    return new BadRequestObjectResult(response);
                };
            });

        return services;
    }
}
