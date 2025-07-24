using System.Net;

namespace GTreasury.Api.Functions.Services.Interface
{
    public interface IDefaultExceptionToHttpMapper
    {
        HttpStatusCode Map(Exception exception);
    }
}
