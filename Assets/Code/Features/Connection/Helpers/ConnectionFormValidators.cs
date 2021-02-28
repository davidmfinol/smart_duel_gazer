using System.Text.RegularExpressions;

namespace AssemblyCSharp.Assets.Code.Features.Connection.Helpers
{
    public class ConnectionFormValidators
    {
        private static readonly Regex IP_ADDRESS_REGEX = new Regex(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");
        private static readonly Regex PORT_REGEX = new Regex("[0-9]+");

        public bool IsValidIpAddress(string ipAddress)
        {
            return IP_ADDRESS_REGEX.IsMatch(ipAddress);
        }

        public bool IsValidPort(string port)
        {
            return PORT_REGEX.IsMatch(port);
        }
    }
}
