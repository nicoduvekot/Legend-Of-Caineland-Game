using System;

namespace GameState.Core
{
    public readonly struct PlayerId : IEquatable<PlayerId>
    {
        public readonly string Value;
        
        public PlayerId(string value)
        {
            Value = value;
        }
        
        public static PlayerId New() => new(Guid.NewGuid().ToString());
        
        public bool Equals(PlayerId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is PlayerId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }
}