using System;

namespace GameState.Core
{
    /// <summary>
    /// This is replaced with the string identifier for a scene name
    /// </summary>
    [Obsolete]
    public readonly struct LevelId : IEquatable<LevelId>
    {
        public readonly string Value;
        
        public LevelId(string value)
        {
            Value = value;
        }
        
        public static LevelId New() => new(Guid.NewGuid().ToString());
        
        public bool Equals(LevelId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is LevelId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }
}