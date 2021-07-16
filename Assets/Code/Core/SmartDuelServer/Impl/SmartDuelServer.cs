using System;
using Code.Core.SmartDuelServer.Interface;
using Code.Core.SmartDuelServer.Interface.Entities;
using Code.Core.SmartDuelServer.Interface.Entities.EventData;
using Code.Wrappers.WrapperWebSocket.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UniRx;
using UnityEngine;

namespace Code.Core.SmartDuelServer.Impl
{
    public class SmartDuelServer : ISmartDuelServer, ISmartDuelEventReceiver
    {
        private readonly IWebSocketFactory _webSocketFactory;

        private IWebSocketProvider _socket;

        private readonly ISubject<SmartDuelEvent> _globalEvents = new Subject<SmartDuelEvent>();
        public IObservable<SmartDuelEvent> GlobalEvents => _globalEvents;

        private readonly ISubject<SmartDuelEvent> _roomEvents = new Subject<SmartDuelEvent>();
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
            switch (scope)
            {
                case SmartDuelEventConstants.GlobalScope:
                    HandleGlobalEvent(action);
                    break;
                case SmartDuelEventConstants.RoomScope:
                    HandleRoomEvent(action, json);
                    break;
                    // TODO:
                    //case SmartDuelEventConstants.cardScope:
                    //    HandleCardEvent(action, json);
                    //    break;
            }
        }

        private void HandleGlobalEvent(string action)
        {
            try
            {
                var e = new SmartDuelEvent(SmartDuelEventConstants.GlobalScope, action);

                _globalEvents.OnNext(e);
            }
            // TODO: check if subject is disposed before calling OnNext
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private void HandleRoomEvent(string action, JToken json)
        {
            try
            {
                var data = RoomEventData.FromJson(json);
                var e = new SmartDuelEvent(SmartDuelEventConstants.RoomScope, action, data);

                _roomEvents.OnNext(e);
            }
            // TODO: check if subject is disposed before calling OnNext
            catch (Exception e)
            {
                Debug.Log(e);
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