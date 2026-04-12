using System;

namespace RedDotSour.Persistence
{
    [Serializable]
    public struct JournalEntry
    {
        public string cat;
        public string key;
        public long ticks;  // 0 = null (미확인)
    }
}
