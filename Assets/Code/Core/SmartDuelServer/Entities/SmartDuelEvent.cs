using System;
using Code.Core.SmartDuelServer.Entities.EventData;

namespace Code.Core.SmartDuelServer.Entities
{
    public class SmartDuelEvent : IEquatable<SmartDuelEvent>
    {
        public string Scope { get; }
        public string Action { get; }
        public SmartDuelEventData Data { get; }

        public SmartDuelEvent(string scope, string action, SmartDuelEventData data = null)
        {
            Scope = scope;
            Action = action;
            Data = data;
        }

        public bool Equals(SmartDuelEvent other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Scope == other.Scope && Action == other.Action && Equals(Data, other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SmartDuelEvent) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Scope != null ? Scope.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Action != null ? Action.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Data != null ? Data.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}