namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection.Entities
{
    public class ConnectionInfo
    {
        public string IpAddress { get; private set; }

        public string Port { get; private set; }

        public ConnectionInfo(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
