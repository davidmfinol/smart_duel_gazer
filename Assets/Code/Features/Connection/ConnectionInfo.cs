using System;

public class ConnectionInfo
{
    public string IpAddress { get; set; }

    public string Port { get; set; }

    public ConnectionInfo(string ipAddress, string port)
    {
        IpAddress = ipAddress;
        Port = port;
    }
}
