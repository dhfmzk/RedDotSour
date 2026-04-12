using System;
using System.Collections.Generic;

namespace RedDotSour.Persistence
{
    [Serializable]
    public class RedDotSaveData
    {
        public List<CategoryData> categories = new();

        [Serializable]
        public class CategoryData
        {
            public string categoryName;
            public List<RecordData> records = new();
        }

        [Serializable]
        public class RecordData
        {
            public string key;
            public long checkedAtTicks;  // DateTime?.Ticks. 0 = null (미확인)
        }
    }
}
