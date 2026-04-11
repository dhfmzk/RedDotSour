using System;
using System.Collections.Generic;

namespace RedDotSour.Core
{
    public class RedDotSour<TCategory> where TCategory : Enum
    {
        private readonly Dictionary<TCategory, IRedDotContainer> _containers = new();

        /// <summary>
        /// 카테고리에 새 컨테이너를 생성한다. 키 타입은 이 시점에 결정된다.
        /// </summary>
        public RedDotContainer<TKey> Create<TKey>(TCategory category)
            where TKey : struct, IEquatable<TKey>
        {
            if (this._containers.ContainsKey(category))
            {
                throw new InvalidOperationException(
                    $"Category '{category}' already has a container.");
            }

            var container = new RedDotContainer<TKey>();
            this._containers.Add(category, container);
            return container;
        }

        /// <summary>
        /// 카테고리의 컨테이너를 타입 캐스팅하여 가져온다.
        /// </summary>
        public RedDotContainer<TKey> Get<TKey>(TCategory category)
            where TKey : struct, IEquatable<TKey>
        {
            if (!this._containers.TryGetValue(category, out var container))
            {
                return null;
            }

            return container as RedDotContainer<TKey>;
        }

        /// <summary>
        /// 특정 카테고리+키가 빨콩 상태인지 확인한다.
        /// </summary>
        public bool IsOn<TKey>(TCategory category, TKey key)
            where TKey : struct, IEquatable<TKey>
        {
            var container = this.Get<TKey>(category);
            return container != null && container.IsOn(key);
        }

        /// <summary>
        /// 카테고리 내 빨콩이 하나라도 있는지 확인한다.
        /// </summary>
        public bool IsOnAny(TCategory category)
        {
            if (this._containers.TryGetValue(category, out var container))
            {
                return container.IsOnAny();
            }

            return false;
        }

        /// <summary>
        /// 카테고리 내 빨콩 개수를 반환한다.
        /// </summary>
        public int CountOn(TCategory category)
        {
            if (this._containers.TryGetValue(category, out var container))
            {
                return container.CountOn();
            }

            return 0;
        }
    }
}
