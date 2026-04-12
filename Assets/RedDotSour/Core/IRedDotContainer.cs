using System;
using System.Collections.Generic;
using RedDotSour.Persistence;

namespace RedDotSour.Core
{
    public interface IRedDotContainer
    {
        bool IsOnAny();
        int CountOn();
        void ClearAll();

        event Action OnChanged;

        int DirtyCount { get; }
        void ClearDirty();

        // 직렬화 지원
        string CategoryName { get; }
        List<RedDotSaveData.RecordData> ExportDirtyRecords();
        List<RedDotSaveData.RecordData> ExportAllRecords();
        void ImportRecords(List<RedDotSaveData.RecordData> records);
    }
}
