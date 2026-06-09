using APITesting.Common.Constants;
using APITesting.Common.Localization;
using APITesting.Common.Responses;
using APITesting.Models.User;
using APITesting.Services.User;
using Microsoft.AspNetCore.Mvc;

namespace APITesting.Controllers;

[Route("api/users")]
[ApiController]
[Tags(tags: "Users")]
public sealed class UserController(
    IUserService service,
    LanguageProvider lang) : ControllerBase
{
    [HttpGet(template: "GetById/{id:guid}")]
    public async Task<ApiResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await ApiResponse.AddAsync(
            valueFactoryAsync: ct => service.GetByIdAsync(id, ct),
            language: lang,
            status: GlobalParams.Lang.Status.Success,
            message: GlobalParams.Lang.Message.Success,
            cancellationToken: cancellationToken
        );
    }

    [HttpGet(template: "GetList")]
    public async Task<ApiResponse> GetListAsync(
        [FromQuery] UserListRequest request,
        CancellationToken cancellationToken)
    {
        return await ApiResponse.AddAsync(
            valueFactoryAsync: ct => service.GetListAsync(request, ct),
            language: lang,
            status: GlobalParams.Lang.Status.Success,
            message: GlobalParams.Lang.Message.Success,
            dataSelector: result => result.Items,
            extraSelector: result => new()
            {
                ["pagination"] = result.ToMeta()
            },
            cancellationToken: cancellationToken
        );
    }

    [HttpPost(template: "Create")]
    public async Task<ApiResponse> CreateAsync(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        return await ApiResponse.AddAsync(
            valueFactoryAsync: ct => service.CreateAsync(request, ct),
            language: lang,
            status: GlobalParams.Lang.Status.Success,
            message: GlobalParams.Lang.Message.Created,
            cancellationToken: cancellationToken
        );
    }

    [HttpPut(template: "Update/{id:guid}")]
    public async Task<ApiResponse> UpdateAsync(
        Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        return await ApiResponse.AddAsync(
            valueFactoryAsync: ct => service.UpdateAsync(id, request, ct),
            language: lang,
            status: GlobalParams.Lang.Status.Success,
            message: GlobalParams.Lang.Message.Updated,
            cancellationToken: cancellationToken);
    }

    [HttpDelete(template: "Delete/{id:guid}")]
    public async Task<ApiResponse> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return await ApiResponse.AddAsync(
            valueFactoryAsync: ct => Task.FromResult((object?)null),
            language: lang,
            status: GlobalParams.Lang.Status.Success,
            message: GlobalParams.Lang.Message.Deleted,
            cancellationToken: cancellationToken
        );
    }
}
