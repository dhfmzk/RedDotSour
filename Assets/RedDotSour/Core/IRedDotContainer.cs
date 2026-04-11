using System;

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
    }
}
