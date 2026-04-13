using System;
using System.Collections.Generic;
using RedDotSour.Persistence;

namespace RedDotSour.Core
{
    /// <summary>
    /// 소비자용 인터페이스. UI 등에서 상태 조회 + 이벤트 구독에 사용.
    /// </summary>
    public interface IRedDotContainer
    {
        string CategoryName { get; }
        bool IsOnAny();
        int CountOn();
        void ClearAll();
        event Action OnChanged;
    }

    /// <summary>
    /// 영속화 내부용 인터페이스. RedDotSour의 Save/Load에서만 사용.
    /// </summary>
    internal interface IRedDotContainerPersistence
    {
        int DirtyCount { get; }
        void ClearDirty();
        List<RedDotSaveData.RecordData> ExportDirtyRecords();
        List<RedDotSaveData.RecordData> ExportAllRecords();
        void ImportRecords(List<RedDotSaveData.RecordData> records);
    }
}
