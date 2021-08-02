using System;

namespace Code.Core.Storage.Connection.Models
{
    public class ConnectionInfoModel : IEquatable<ConnectionInfoModel>
    {
        public string IpAddress { get; }
        public string Port { get; }

        public ConnectionInfoModel(string ipAddress, string port)
        {
            IpAddress = ipAddress;
            Port = port;
        }

        public bool Equals(ConnectionInfoModel other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IpAddress == other.IpAddress && Port == other.Port;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ConnectionInfoModel) obj);
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