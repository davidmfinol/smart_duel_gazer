using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Interface;
using Dpoch.SocketIO;

namespace AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Impl
{
    public class WebSocketProvider : IWebSocketProvider
    {
        private readonly SocketIO _socket;

        ISmartDuelEventReceiver _receiver;

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

        public void EmitEvent(string eventName, IDictionary<string, object> data)
        {
            _socket.Emit(eventName, data);
        }

        public void Dispose()
        {
            _socket.Close();
        }

        private void OnEventReceived(string scope, string action, SocketIOEvent e)
        {
            _receiver?.OnEventReceived(scope, action, e.Data[0]);
        }

        private void RegisterGlobalHandlers()
        {
            var scope = SmartDuelEventConstants.globalScope;

            _socket.On(SmartDuelEventConstants.globalConnectAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalConnectAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalConnectErrorAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalConnectErrorAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalConnectTimeoutAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalConnectTimeoutAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalConnectingAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalConnectingAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalDisconnectAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalDisconnectAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalErrorAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalErrorAction, e);
            });

            _socket.On(SmartDuelEventConstants.globalReconnectAction, e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.globalReconnectAction, e);
            });
        }

        private void RegisterRoomHandlers()
        {
            var scope = SmartDuelEventConstants.roomScope;

            _socket.On($"{scope}:{SmartDuelEventConstants.roomCloseAction}", e =>
            {
                OnEventReceived(scope, SmartDuelEventConstants.roomCloseAction, e);
            });
        }

        private void RegisterCardHandlers()
        {
            var scope = SmartDuelEventConstants.cardScope;

            _socket.On($"{scope}:{SmartDuelEventConstants.cardPlayAction}", e => {
                OnEventReceived(scope, SmartDuelEventConstants.cardPlayAction, e);
            });

            _socket.On($"{scope}:{SmartDuelEventConstants.cardRemoveAction}", e => {
                OnEventReceived(scope, SmartDuelEventConstants.cardRemoveAction, e);
            });
        }
    }
}