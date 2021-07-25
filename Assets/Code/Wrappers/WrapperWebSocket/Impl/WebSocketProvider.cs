using Code.Core.SmartDuelServer.Interface.Entities;
using Code.Wrappers.WrapperWebSocket.Interface;
using Dpoch.SocketIO;
using UnityEngine;

namespace Code.Wrappers.WrapperWebSocket.Impl
{
    public class WebSocketProvider : IWebSocketProvider
    {
        private readonly SocketIO _socket;

        private ISmartDuelEventReceiver _receiver;

        public WebSocketProvider(SocketIO socket)
        {
            _socket = socket;
        }

        public void Init(ISmartDuelEventReceiver receiver)
        {
            _receiver = receiver;

            RegisterGlobalHandlers();
            RegisterRoomHandlers();
            RegisterCardHandlers();

            _socket.Connect();
        }

        public void EmitEvent(string eventName, string json)
        {
            Debug.Log($"EmitEvent(eventName: {eventName})");
            
            _socket.Emit(eventName, json);
        }

        public void Dispose()
        {
            Debug.Log("Dispose()");
            
            _socket.Close();
        }

        private void OnEventReceived(string scope, string action, SocketIOEvent e = null)
        {
            Debug.Log($"OnEventReceived(scope: {scope}, action: {action})");
            
            _receiver?.OnEventReceived(scope, action, e?.Data[0]);
        }

        private void RegisterGlobalHandlers()
        {
            const string scope = SmartDuelEventConstants.GlobalScope;

            _socket.OnOpen += () => OnEventReceived(scope, SmartDuelEventConstants.GlobalConnectAction);
            _socket.OnConnectFailed += () => OnEventReceived(scope, SmartDuelEventConstants.GlobalConnectErrorAction);
            _socket.OnError += _ => OnEventReceived(scope, SmartDuelEventConstants.GlobalErrorAction);
        }

        private void RegisterRoomHandlers()
        {
            const string scope = SmartDuelEventConstants.RoomScope;

            RegisterHandler(scope, SmartDuelEventConstants.RoomGetDuelistsAction);
            RegisterHandler(scope, SmartDuelEventConstants.RoomSpectateAction);
            RegisterHandler(scope, SmartDuelEventConstants.RoomStartAction);
            RegisterHandler(scope, SmartDuelEventConstants.RoomCloseAction);
        }

        private void RegisterCardHandlers()
        {
            const string scope = SmartDuelEventConstants.CardScope;
            
            RegisterHandler(scope, SmartDuelEventConstants.CardPlayAction);
            RegisterHandler(scope, SmartDuelEventConstants.CardRemoveAction);
        }

        private void RegisterHandler(string scope, string action)
        {
            var eventName = scope == SmartDuelEventConstants.GlobalScope ? action : $"{scope}:{action}";

            _socket.On(eventName, e =>
            {
                OnEventReceived(scope, action, e);
            }); 
        }
    }
}