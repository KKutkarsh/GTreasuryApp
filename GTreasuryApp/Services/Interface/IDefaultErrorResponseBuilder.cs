using GTreasury.Api.Utilities.Dtos;

namespace GTreasury.Api.Functions.Services.Interface
{
    public interface IDefaultErrorResponseBuilder
    {
        ErrorResponse Build(Exception exception);
    }
}
