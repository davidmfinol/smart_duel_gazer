using System;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using Dpoch.SocketIO;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl
{
    public class SmartDuelServer : ISmartDuelServer
    {
        private const string CONNECTION_URL = "ws://{0}:{1}/socket.io/?EIO=3&transport=websocket";
        private const string SUMMON_EVENT_NAME = "summonEvent";
        private const string REMOVE_CARD_EVENT = "removeCardEvent";
        private const string POSITION_CHANGE_EVENT = "positionChangeEvent";

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
            _socket.On(SUMMON_EVENT_NAME, OnSummonEventReceived);
            _socket.On(REMOVE_CARD_EVENT, OnRemoveCardEventReceived);
            _socket.On(POSITION_CHANGE_EVENT, OnPositionChangeEventReceived);

            _socket.Connect();
        }

        public void Dispose()
        {
            _socket?.Close();
            _socket = null;
            _listener = null;
        }

        private void OnSummonEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnSummonEventReceived(SocketIOEvent: {e})");

            var data = e.Data[0];
            var cardId = data["yugiohCardId"].ToString().RemoveQuotes();
            var zoneName = data["zoneName"].ToString().RemoveQuotes();
            bool isDefenceMode = CheckIfTrue(data["battleMode"].ToString().RemoveQuotes());
            bool isSet = CheckIfTrue(data["faceMode"].ToString().RemoveQuotes());

            _listener.onSmartDuelEventReceived(new SummonEvent(cardId, zoneName, isDefenceMode, isSet));
        }

        private void OnRemoveCardEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnRemoveCardEventReceived(SocketIOEvent: {e})");

            var data = e.Data[0];
            var zoneName = data["zoneName"].ToString().RemoveQuotes();
            bool isDefenceMode = CheckIfTrue(data["battleMode"].ToString().RemoveQuotes());
            bool isSet = CheckIfTrue(data["faceMode"].ToString().RemoveQuotes());

            _listener.onSmartDuelEventReceived(new RemoveCardEvent(zoneName, isDefenceMode, isSet));
        }

        private void OnPositionChangeEventReceived(SocketIOEvent e)
        {
            Debug.Log($"OnPositionChangeEventReceived(SocketIOEvent: {e})");

            var data = e.Data[0];
            var zoneName = data["zoneName"].ToString().RemoveQuotes();
            bool isDefenceMode = CheckIfTrue(data["battleMode"].ToString().RemoveQuotes());
            bool isSet = CheckIfTrue(data["faceMode"].ToString().RemoveQuotes());

            _listener.onSmartDuelEventReceived(new PositionChangeEvent(zoneName, isDefenceMode, isSet));
        }

        private bool CheckIfTrue(string mode)
        {
            if (mode == "0")
            {
                return true;
            }

            return false;
        }
    }
}