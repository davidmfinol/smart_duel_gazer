namespace AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection.Models
{
    public class ConnectionInfoModel
    {
        public string IpAddress { get; set; }

        public string Port { get; set; }

        public ConnectionInfoModel(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }
    }
}
