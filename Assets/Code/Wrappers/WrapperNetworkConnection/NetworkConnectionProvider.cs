using UnityEngine;

namespace Code.Wrappers.WrapperNetworkConnection
{
    public interface INetworkConnectionProvider
    {
        bool IsConnected();
        bool HasWifi();
    }

    public class NetworkConnectionProvider : INetworkConnectionProvider
    {
        public bool IsConnected()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        public bool HasWifi()
        {
            return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
        }
    }
}