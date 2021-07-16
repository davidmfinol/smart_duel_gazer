using System;
using Code.Core.SmartDuelServer.Interface.Entities;

namespace Code.Core.SmartDuelServer.Interface
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
}