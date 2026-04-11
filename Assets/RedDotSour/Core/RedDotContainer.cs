using System;
using System.Collections.Generic;

namespace RedDotSour.Core
{
    public class RedDotContainer<TKey> : IRedDotContainer
        where TKey : struct, IEquatable<TKey>
    {
        private readonly Dictionary<TKey, DateTime?> _table = new();

        /// <summary>
        /// 키를 등록한다. 미확인(null) 상태로 추가되어 빨콩이 켜진다.
        /// 이미 존재하면 무시한다.
        /// </summary>
        public void Register(TKey key)
        {
            this._table.TryAdd(key, null);
        }

        /// <summary>
        /// 확인 처리. 현재 시점으로 마킹하여 빨콩을 끈다.
        /// </summary>
        public void Mark(TKey key)
        {
            this._table[key] = DateTime.Now;
        }

        /// <summary>
        /// 특정 시점으로 확인 처리.
        /// </summary>
        public void Mark(TKey key, DateTime at)
        {
            this._table[key] = at;
        }

        /// <summary>
        /// 미확인 상태로 되돌린다. 빨콩이 다시 켜진다.
        /// </summary>
        public void Unmark(TKey key)
        {
            this._table[key] = null;
        }

        /// <summary>
        /// 키를 제거한다.
        /// </summary>
        public bool Remove(TKey key)
        {
            return this._table.Remove(key);
        }

        /// <summary>
        /// 해당 키가 빨콩 상태인지 확인한다.
        /// 테이블에 존재하고 값이 null이면 빨콩.
        /// </summary>
        public bool IsOn(TKey key)
        {
            return this._table.TryGetValue(key, out var value) && value == null;
        }

        /// <summary>
        /// 해당 키의 마지막 확인 시간을 가져온다.
        /// </summary>
        public bool TryGet(TKey key, out DateTime? value)
        {
            return this._table.TryGetValue(key, out value);
        }

        /// <summary>
        /// 컨테이너 내 빨콩이 하나라도 있는지 확인한다.
        /// </summary>
        public bool IsOnAny()
        {
            foreach (var kv in this._table)
            {
                if (kv.Value == null) return true;
            }

            return false;
        }

        /// <summary>
        /// 컨테이너 내 빨콩 개수를 반환한다.
        /// </summary>
        public int CountOn()
        {
            var count = 0;
            foreach (var kv in this._table)
            {
                if (kv.Value == null) count++;
            }

            return count;
        }

        /// <summary>
        /// 모든 키를 제거한다.
        /// </summary>
        public void ClearAll()
        {
            this._table.Clear();
        }
    }
}
