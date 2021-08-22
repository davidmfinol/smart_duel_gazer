namespace Code.Core.Config.Entities
{
    public interface IAppConfig
    {
        string SmartDuelServerAddress { get; }
        string SmartDuelServerPort { get; }
    }

    public class AppConfig : IAppConfig
    {
        public string SmartDuelServerAddress => "https://smart-duel-server.herokuapp.com";
        public string SmartDuelServerPort => "443";
    }
}