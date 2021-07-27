namespace Code.Core.Storage.Connection.Models
{
    public class ConnectionInfoModel
    {
        public string IpAddress { get; }

        public string Port { get; }

        public ConnectionInfoModel(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
