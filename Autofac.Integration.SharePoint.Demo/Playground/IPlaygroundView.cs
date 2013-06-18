namespace Autofac.Integration.SharePoint.Demo.Playground
{
    public interface IPlaygroundView
    {
        bool IsPostback { get; }
        string SuccessMessage { get; set; }
        string ErrorMessage { get; set; }
    }
}