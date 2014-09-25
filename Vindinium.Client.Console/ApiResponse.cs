using Vindinium.Common.Entities;

namespace Vindinium.Client.Console
{
    public class ApiResponse : IApiResponse
    {
        #region IApiResponse Members

        public string ErrorMessage { get; set; }
        public string Text { get; set; }
        public bool HasError { get; set; }

        #endregion
    }
}