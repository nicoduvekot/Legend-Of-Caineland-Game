using System;

namespace CoinManagement
{
    /// <summary>
    /// This defines a unique id for each coin. Making every coin game object uniquely identifiable.
    /// </summary>
    [Serializable]
    public readonly struct CoinId : IEquatable<CoinId>
    {
        public readonly string Value;
        
        private CoinId(string value) => Value = value;

        public static CoinId New() => new(Guid.NewGuid().ToString());
        
        public bool Equals(CoinId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is CoinId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;
    }
}