using System;
using System.Collections.Generic;
using RedDotSour.Persistence;

namespace RedDotSour.Core
{
    public class RedDotSour<TCategory> where TCategory : Enum
    {
        private readonly Dictionary<TCategory, IRedDotContainer> _containers = new();
        private IRedDotPersistence _persistence;

        /// <summary>
        /// 영속화 구현체를 설정한다.
        /// </summary>
        public void SetPersistence(IRedDotPersistence persistence)
        {
            this._persistence = persistence;
        }

        /// <summary>
        /// 카테고리에 새 컨테이너를 생성한다. 키 타입과 시리얼라이저는 이 시점에 결정된다.
        /// </summary>
        public RedDotContainer<TKey> Create<TKey>(
            TCategory category,
            Func<TKey, string> keySerializer,
            Func<string, TKey> keyDeserializer)
            where TKey : struct, IEquatable<TKey>
        {
            if (this._containers.ContainsKey(category))
            {
                throw new InvalidOperationException(
                    $"Category '{category}' already has a container.");
            }

            var container = new RedDotContainer<TKey>(
                category.ToString(),
                keySerializer,
                keyDeserializer);
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

            if (container is not RedDotContainer<TKey> typed)
            {
                throw new InvalidCastException(
                    $"Category '{category}' has key type mismatch. Expected {typeof(TKey).Name}.");
            }

            return typed;
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

        #region Persistence

        /// <summary>
        /// dirty records만 저장한다.
        /// </summary>
        public void Save()
        {
            if (this._persistence == null) return;

            var data = this.ExportDirtySaveData();
            this._persistence.Save(data);

            foreach (var container in this._containers.Values)
            {
                container.ClearDirty();
            }
        }

        /// <summary>
        /// 전체 데이터를 저장한다. Compact용.
        /// </summary>
        public void Compact()
        {
            if (this._persistence == null) return;

            var data = this.ExportAllSaveData();
            this._persistence.SaveAll(data);

            foreach (var container in this._containers.Values)
            {
                container.ClearDirty();
            }
        }

        /// <summary>
        /// 저장된 데이터를 로드한다. 기존 컨테이너가 등록되어 있어야 한다.
        /// </summary>
        public void Load()
        {
            if (this._persistence == null) return;

            var data = this._persistence.Load();
            this.ImportSaveData(data);
        }

        private RedDotSaveData ExportDirtySaveData()
        {
            var data = new RedDotSaveData();
            foreach (var container in this._containers.Values)
            {
                if (container.DirtyCount == 0) continue;

                data.categories.Add(new RedDotSaveData.CategoryData
                {
                    categoryName = container.CategoryName,
                    records = container.ExportDirtyRecords()
                });
            }

            return data;
        }

        private RedDotSaveData ExportAllSaveData()
        {
            var data = new RedDotSaveData();
            foreach (var container in this._containers.Values)
            {
                data.categories.Add(new RedDotSaveData.CategoryData
                {
                    categoryName = container.CategoryName,
                    records = container.ExportAllRecords()
                });
            }

            return data;
        }

        private void ImportSaveData(RedDotSaveData data)
        {
            var nameToContainer = new Dictionary<string, IRedDotContainer>();
            foreach (var kv in this._containers)
            {
                nameToContainer[kv.Value.CategoryName] = kv.Value;
            }

            foreach (var catData in data.categories)
            {
                if (nameToContainer.TryGetValue(catData.categoryName, out var container))
                {
                    container.ImportRecords(catData.records);
                }
            }
        }

        #endregion
    }
}
