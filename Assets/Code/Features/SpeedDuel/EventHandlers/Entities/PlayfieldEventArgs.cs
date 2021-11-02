using System;
using System.Collections.Generic;

namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public abstract class PlayfieldEventArgs
    {
    }

    public class PlayfieldEventValue<T> : PlayfieldEventArgs, IEquatable<PlayfieldEventValue<T>>
    {
        public T Value { get; }

        public PlayfieldEventValue(T value)
        {
            Value = value;
        }

        public bool Equals(PlayfieldEventValue<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || EqualityComparer<T>.Default.Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PlayfieldEventValue<T>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<T>.Default.GetHashCode(Value);
        }
    }
}