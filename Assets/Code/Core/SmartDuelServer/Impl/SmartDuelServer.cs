using System;
using UnityEngine;
using Dpoch.SocketIO;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl
{
    public class SmartDuelServer : ISmartDuelServer
    {
        private const string ConnectionUrl = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";
        private const string PlayCardEventName = "card:play";
        private const string RemoveCardEventName = "card:remove";

        private IDataManager _dataManager;

        private ISmartDuelEventListener _listener;
        private SocketIO _socket;

        public SmartDuelServer(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void Connect(ISmartDuelEventListener listener)
        {
            if (_socket != null)
            {
                throw new Exception("There is already a socket that has not been closed yet!");
            }

            _listener = listener;

            var connectionInfo = _dataManager.GetConnectionInfo();
            var url = string.Format(ConnectionUrl, connectionInfo?.IpAddress, connectionInfo?.Port);

            _socket = new SocketIO(url);

            _socket.OnOpen += () => Debug.Log("Socket open!");
            _socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
            _socket.OnClose += () => Debug.Log("Socket closed!");
            _socket.OnError += (err) => Debug.Log($"Socket Error: {err}");
            _socket.On(PlayCardEventName, OnPlayCardEventReceived);
            _socket.On(RemoveCardEventName, OnRemoveCardEventReceived);

            _socket.Connect();
        }

        public void Dispose()
        {
            _socket?.Close();
            _socket = null;
            _listener = null;
        }

        private void OnPlayCardEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnPlayCardEventReceived(SocketIOEvent: {e})");

            _listener.OnSmartDuelEventReceived(PlayCardEvent.FromJson(e.Data[0]));
        }

        private void OnRemoveCardEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnRemoveCardEventReceived(SocketIOEvent: {e})");

            _listener.OnSmartDuelEventReceived(RemoveCardEvent.FromJson(e.Data[0]));
        }
    }
}