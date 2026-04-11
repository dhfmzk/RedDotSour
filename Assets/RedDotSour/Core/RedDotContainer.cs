using System;
using System.Collections.Generic;

namespace RedDotSour.Core
{
    public class RedDotContainer<TKey> : IRedDotContainer
        where TKey : struct, IEquatable<TKey>
    {
        private readonly Dictionary<TKey, DateTime?> _table = new();
        private readonly HashSet<TKey> _dirtyKeys = new();
        private int _onCount;

        public event Action OnChanged;

        public int DirtyCount => this._dirtyKeys.Count;

        /// <summary>
        /// 키를 등록한다. 미확인(null) 상태로 추가되어 빨콩이 켜진다.
        /// 이미 존재하면 무시한다.
        /// </summary>
        public void Register(TKey key)
        {
            if (!this._table.TryAdd(key, null))
            {
                return;
            }

            this._onCount++;
            this._dirtyKeys.Add(key);
            this.RaiseOnChanged();
        }

        /// <summary>
        /// 확인 처리. 현재 시점으로 마킹하여 빨콩을 끈다.
        /// </summary>
        public void Mark(TKey key)
        {
            this.Mark(key, DateTime.Now);
        }

        /// <summary>
        /// 특정 시점으로 확인 처리.
        /// </summary>
        public void Mark(TKey key, DateTime at)
        {
            if (!this._table.TryGetValue(key, out var current))
            {
                throw new KeyNotFoundException($"Key '{key}' is not registered.");
            }

            if (current == null)
            {
                this._onCount--;
            }

            this._table[key] = at;
            this._dirtyKeys.Add(key);
            this.RaiseOnChanged();
        }

        /// <summary>
        /// 미확인 상태로 되돌린다. 빨콩이 다시 켜진다.
        /// </summary>
        public void Unmark(TKey key)
        {
            if (!this._table.TryGetValue(key, out var current))
            {
                throw new KeyNotFoundException($"Key '{key}' is not registered.");
            }

            if (current == null)
            {
                return;
            }

            this._table[key] = null;
            this._onCount++;
            this._dirtyKeys.Add(key);
            this.RaiseOnChanged();
        }

        /// <summary>
        /// 키를 제거한다.
        /// </summary>
        public bool Remove(TKey key)
        {
            if (!this._table.TryGetValue(key, out var current))
            {
                return false;
            }

            if (current == null)
            {
                this._onCount--;
            }

            this._table.Remove(key);
            this._dirtyKeys.Remove(key);
            this.RaiseOnChanged();
            return true;
        }

        /// <summary>
        /// 해당 키가 빨콩 상태인지 확인한다.
        /// </summary>
        public bool IsOn(TKey key)
        {
            return this._table.TryGetValue(key, out var value) && value == null;
        }

        /// <summary>
        /// 해당 키의 레코드를 가져온다. 스택 할당 readonly struct 반환.
        /// </summary>
        public bool TryGet(TKey key, out RedDotRecord<TKey> record)
        {
            if (!this._table.TryGetValue(key, out var checkedAt))
            {
                record = default;
                return false;
            }

            record = new RedDotRecord<TKey>(key, checkedAt);
            return true;
        }

        /// <summary>
        /// 컨테이너 내 빨콩이 하나라도 있는지 확인한다. O(1).
        /// </summary>
        public bool IsOnAny()
        {
            return this._onCount > 0;
        }

        /// <summary>
        /// 컨테이너 내 빨콩 개수를 반환한다. O(1).
        /// </summary>
        public int CountOn()
        {
            return this._onCount;
        }

        /// <summary>
        /// 모든 키를 제거한다.
        /// </summary>
        public void ClearAll()
        {
            this._table.Clear();
            this._dirtyKeys.Clear();
            this._onCount = 0;
            this.RaiseOnChanged();
        }

        /// <summary>
        /// 변경된 레코드만 반환한다.
        /// </summary>
        public IEnumerable<RedDotRecord<TKey>> GetDirtyRecords()
        {
            foreach (var key in this._dirtyKeys)
            {
                if (this._table.TryGetValue(key, out var checkedAt))
                {
                    yield return new RedDotRecord<TKey>(key, checkedAt);
                }
            }
        }

        /// <summary>
        /// 전체 레코드를 반환한다.
        /// </summary>
        public IEnumerable<RedDotRecord<TKey>> GetAllRecords()
        {
            foreach (var kv in this._table)
            {
                yield return new RedDotRecord<TKey>(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// 외부(DB/파일)에서 로드한 레코드를 적재한다. dirty 마킹 안 함.
        /// </summary>
        public void LoadRecord(TKey key, DateTime? checkedAt)
        {
            this._table[key] = checkedAt;

            if (checkedAt == null)
            {
                this._onCount++;
            }
        }

        /// <summary>
        /// dirty 플래그를 리셋한다. Save 후 호출. O(1).
        /// </summary>
        public void ClearDirty()
        {
            this._dirtyKeys.Clear();
        }

        private void RaiseOnChanged()
        {
            if (this.OnChanged == null)
            {
                return;
            }

            foreach (var handler in this.OnChanged.GetInvocationList())
            {
                try
                {
                    ((Action)handler).Invoke();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }
}
