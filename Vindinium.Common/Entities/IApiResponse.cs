namespace Vindinium.Common.Entities
{
    public interface IApiResponse
    {
        string ErrorMessage { get; set; }
        string Text { get; set; }
        bool HasError { get; set; }
    }
}