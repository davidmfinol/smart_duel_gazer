using System;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface;
using AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Interface.Entities;
using AssemblyCSharp.Assets.Code.Wrappers.WrapperWebSocket.Interface;
using Newtonsoft.Json.Linq;
using UniRx;

namespace AssemblyCSharp.Assets.Code.Core.SmartDuelServer.Impl
{
    public class SmartDuelServer : ISmartDuelServer, ISmartDuelEventReceiver
    {
        private readonly IWebSocketFactory _webSocketFactory;

        private IWebSocketProvider _socket;

        private ISubject<SmartDuelEvent> _globalEvents = new Subject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> GlobalEvents => _globalEvents;

        private ISubject<SmartDuelEvent> _roomEvents = new Subject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> RoomEvents => _roomEvents;

        private ReplaySubject<SmartDuelEvent> _cardEvents = new ReplaySubject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> CardEvents => _cardEvents;

        public SmartDuelServer(IWebSocketFactory webSocketFactory)
        {
            _webSocketFactory = webSocketFactory;
        }

        public void Init()
        {
            if (_socket != null)
            {
                return;
            }

            _socket = _webSocketFactory.CreateWebSocketProvider();
            _socket.Init(this);
        }

        public void EmitEvent(SmartDuelEvent e)
        {
            // TODO:
            _socket.EmitEvent($"{e.Scope}:{e.Action}", null);
        }

        public void OnEventReceived(string scope, string action, JToken json)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _socket?.Dispose();
            _socket = null;

            _cardEvents.Dispose();
            _cardEvents = new ReplaySubject<SmartDuelEvent>();
        }
    }
}