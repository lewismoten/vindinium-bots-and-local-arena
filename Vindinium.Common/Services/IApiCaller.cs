using Vindinium.Common.Entities;

namespace Vindinium.Common.Services
{
    public interface IApiCaller
    {
        void Call(IApiRequest apiRequest, IApiResponse apiResponse);
    }
}