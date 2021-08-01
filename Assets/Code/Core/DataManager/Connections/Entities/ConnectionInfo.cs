namespace Code.Core.DataManager.Connections.Entities
{
    public class ConnectionInfo
    {
        public string IpAddress { get; }

        public string Port { get; }

        public ConnectionInfo(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
