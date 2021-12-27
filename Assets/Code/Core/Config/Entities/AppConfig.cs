namespace Code.Core.Config.Entities
{
    public interface IAppConfig
    {
        bool UseSecureSdsConnection { get; }
        string SdsAddress { get; }
        string SdsPort { get; }
        string SecureSdsAddress { get; }
        string SecureSdsPort { get; }
    }

    public class AppConfig : IAppConfig
    {
        public bool UseSecureSdsConnection => true;
        public string SdsAddress => "http://smart-duel-server.herokuapp.com";
        public string SdsPort => "80";
        public string SecureSdsAddress => "https://smart-duel-server.herokuapp.com";
        public string SecureSdsPort => "443";
    }
}