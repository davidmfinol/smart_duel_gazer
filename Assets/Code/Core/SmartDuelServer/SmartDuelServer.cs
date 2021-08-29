using System;
using Code.Core.Logger;
using Code.Core.SmartDuelServer.Entities;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Wrappers.WrapperWebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UniRx;

namespace Code.Core.SmartDuelServer
{
    public interface ISmartDuelServer
    {
        IObservable<SmartDuelEvent> GlobalEvents { get; }
        IObservable<SmartDuelEvent> RoomEvents { get; }
        IObservable<SmartDuelEvent> CardEvents { get; }
        void Init();
        void EmitEvent(SmartDuelEvent e);
        void Dispose();
    }
    
    public class SmartDuelServer : ISmartDuelServer, ISmartDuelEventReceiver
    {
        private const string Tag = "SmartDuelServer";
        
        private readonly IWebSocketFactory _webSocketFactory;
        private readonly IAppLogger _logger;

        private IWebSocketProvider _socket;

        private readonly ISubject<SmartDuelEvent> _globalEvents = new Subject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> GlobalEvents => _globalEvents;

        private readonly ISubject<SmartDuelEvent> _roomEvents = new Subject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> RoomEvents => _roomEvents;

        private ReplaySubject<SmartDuelEvent> _cardEvents = new ReplaySubject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> CardEvents => _cardEvents;

        public SmartDuelServer(
            IWebSocketFactory webSocketFactory,
            IAppLogger logger)
        {
            _webSocketFactory = webSocketFactory;
            _logger = logger;
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
            // TODO: can be improved
            var json = JsonConvert.SerializeObject(e.Data, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            });

            _socket.EmitEvent($"{e.Scope}:{e.Action}", json);
        }

        #region Receive smart duel events

        public void OnEventReceived(string scope, string action, JToken json)
        {
            _logger.Log(Tag, $"OnEventReceived(scope: {scope}, action: {action}, json: {json})");
            
            switch (scope)
            {
                case SmartDuelEventConstants.GlobalScope:
                    HandleGlobalEvent(action);
                    break;
                case SmartDuelEventConstants.RoomScope:
                    HandleRoomEvent(action, json);
                    break;
                case SmartDuelEventConstants.CardScope:
                    HandleCardEvent(action, json);
                    break;
            }
        }

        private void HandleGlobalEvent(string action)
        {
            _logger.Log(Tag, $"HandleGlobalEvent(action: {action})");
            
            try
            {
                var e = new SmartDuelEvent(SmartDuelEventConstants.GlobalScope, action);
                _globalEvents.OnNext(e);
            }
            catch (Exception e)
            {
                _logger.Exception(Tag, "An error occurred while handling a global event", e);
            }
        }

        private void HandleRoomEvent(string action, JToken json)
        {
            _logger.Log(Tag, $"HandleRoomEvent(action: {action})");
            
            try
            {
                var data = JsonConvert.DeserializeObject<RoomEventData>(json.ToString());
                var e = new SmartDuelEvent(SmartDuelEventConstants.RoomScope, action, data);
                _roomEvents.OnNext(e);
            }
            catch (Exception e)
            {
                _logger.Exception(Tag, "An error occurred while handling a room event", e);
            }
        }

        private void HandleCardEvent(string action, JToken json)
        {
            _logger.Log(Tag, $"HandleCardEvent(action: {action})");

            try
            {
                var data = JsonConvert.DeserializeObject<CardEventData>(json.ToString());
                var e = new SmartDuelEvent(SmartDuelEventConstants.CardScope, action, data);
                _cardEvents.OnNext(e);
            }
            catch (Exception e)
            {
                _logger.Exception(Tag, "An error occurred while handling a card event", e);
            }
        }

        #endregion

        public void Dispose()
        {
            _socket?.Dispose();
            _socket = null;

            _cardEvents.Dispose();
            _cardEvents = new ReplaySubject<SmartDuelEvent>();
        }
    }
}