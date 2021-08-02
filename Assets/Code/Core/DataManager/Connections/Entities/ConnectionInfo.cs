using System;

namespace Code.Core.DataManager.Connections.Entities
{
    public class ConnectionInfo : IEquatable<ConnectionInfo>
    {
        public string IpAddress { get; }
        public string Port { get; }

        public ConnectionInfo(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public bool Equals(ConnectionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IpAddress == other.IpAddress && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ConnectionInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((IpAddress != null ? IpAddress.GetHashCode() : 0) * 397) ^ (Port != null ? Port.GetHashCode() : 0);
            }
        }
    }
}