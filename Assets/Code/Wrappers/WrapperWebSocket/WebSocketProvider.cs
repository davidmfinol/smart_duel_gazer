using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities;
using Dpoch.SocketIO;

namespace Code.Wrappers.WrapperWebSocket
{
    public interface IWebSocketProvider
    {
        void Init(ISmartDuelEventReceiver receiver);
        void EmitEvent(string eventName, string json);
        void Dispose();
    }

    public class WebSocketProvider : IWebSocketProvider
    {
        private const string Tag = "WebSocketProvider";

        private readonly SocketIO _socket;
        private readonly IAppLogger _logger;

        private ISmartDuelEventReceiver _receiver;

        public WebSocketProvider(
            SocketIO socket,
            IAppLogger logger)
        {
            _socket = socket;
            _logger = logger;
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
            _logger.Log(Tag, $"EmitEvent(eventName: {eventName})");

            _socket.Emit(eventName, json);
        }

        public void Dispose()
        {
            _logger.Log(Tag, "Dispose()");

            _socket.Close();
        }

        private void OnEventReceived(string scope, string action, SocketIOEvent e = null)
        {
            _logger.Log(Tag, $"OnEventReceived(scope: {scope}, action: {action})");

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
            RegisterHandler(scope, SmartDuelEventConstants.CardAttackAction);
        }

        private void RegisterHandler(string scope, string action)
        {
            var eventName = scope == SmartDuelEventConstants.GlobalScope ? action : $"{scope}:{action}";

            _socket.On(eventName, e => { OnEventReceived(scope, action, e); });
        }
    }
}