using System;

namespace Code.Features.SpeedDuel.Models
{
    public class PlayfieldTransformValues : IEquatable<PlayfieldTransformValues>
    {
        public float Scale { get; }
        public float YAxisRotation { get; }

        public PlayfieldTransformValues(float scale, float yAxisRotation)
        {
            Scale = scale;
            YAxisRotation = yAxisRotation;
        }

        public bool Equals(PlayfieldTransformValues other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Scale.Equals(other.Scale) && YAxisRotation.Equals(other.YAxisRotation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PlayfieldTransformValues)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Scale.GetHashCode() * 397) ^ YAxisRotation.GetHashCode();
            }
        }
    }
}