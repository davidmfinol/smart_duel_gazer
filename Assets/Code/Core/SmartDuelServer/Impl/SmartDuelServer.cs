using System;
using UnityEngine;
using Dpoch.SocketIO;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl
{
    public class SmartDuelServer : ISmartDuelServer
    {
        private const string CONNECTION_URL = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";
        private const string PLAY_CARD_EVENT = "card:play";
        private const string REMOVE_CARD_EVENT = "card:remove";

        private IDataManager _dataManager;

        private ISmartDuelEventListener _listener;
        private SocketIO _socket;

        public SmartDuelServer(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void Connect(ISmartDuelEventListener listener)
        {
            if (_socket != null || listener == null)
            {
                throw new Exception("There is already a socket that has not been closed yet!");
            }

            _listener = listener;

            var connectionInfo = _dataManager.GetConnectionInfo();
            var url = string.Format(CONNECTION_URL, connectionInfo?.IpAddress, connectionInfo?.Port);

            _socket = new SocketIO(url);

            _socket.OnOpen += () => Debug.Log("Socket open!");
            _socket.OnConnectFailed += () => Debug.Log("Socket failed to connect!");
            _socket.OnClose += () => Debug.Log("Socket closed!");
            _socket.OnError += (err) => Debug.Log($"Socket Error: {err}");
            _socket.On(PLAY_CARD_EVENT, OnPlayCardEventReceived);
            _socket.On(REMOVE_CARD_EVENT, OnRemoveCardEventReceived);

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

            var data = e.Data[0];
            var cardId = data["cardId"].ToString().RemoveQuotes();
            var zoneName = data["zoneName"].ToString().RemoveQuotes();
            var cardPosition = data["cardPosition"].ToString().RemoveQuotes();

            _listener.onSmartDuelEventReceived(new PlayCardEvent(cardId, zoneName, cardPosition));
        }

        private void OnRemoveCardEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnRemoveCardEventReceived(SocketIOEvent: {e})");

            var data = e.Data[0];
            var zoneName = data["zoneName"].ToString().RemoveQuotes();

            _listener.onSmartDuelEventReceived(new RemoveCardEvent(zoneName));
        }
    }
}