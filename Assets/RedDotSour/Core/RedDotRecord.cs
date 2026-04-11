using System;

namespace RedDotSour.Core
{
    public readonly struct RedDotRecord<TKey>
        where TKey : struct, IEquatable<TKey>
    {
        public readonly TKey Key;
        public readonly DateTime? CheckedAt;
        public bool IsOn => this.CheckedAt == null;

        internal RedDotRecord(TKey key, DateTime? checkedAt)
        {
            this.Key = key;
            this.CheckedAt = checkedAt;
        }
    }
}
