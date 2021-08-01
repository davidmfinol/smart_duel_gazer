using System.Text.RegularExpressions;

namespace Code.Features.Connection.Helpers
{
    public class ConnectionFormValidators
    {
        private static readonly Regex IpAddressRegex = new Regex(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$");
        private static readonly Regex PortRegex = new Regex("[0-9]+");

        public bool IsValidIpAddress(string ipAddress)
        {
            return IpAddressRegex.IsMatch(ipAddress);
        }

        public bool IsValidPort(string port)
        {
            return PortRegex.IsMatch(port);
        }
    }
}
