using UnityEngine;

namespace RedDotSour.Persistence
{
    /// <summary>
    /// PlayerPrefs 기반 영속화. 소규모 데이터(~100건) 전용.
    /// 주의: WebGL 1MB 제한, Windows 레지스트리 저장, Android 메인 스레드 블로킹.
    /// </summary>
    public class PlayerPrefsPersistence : IRedDotPersistence
    {
        private readonly string _key;

        public PlayerPrefsPersistence(string key = "RedDotSour_Data")
        {
            this._key = key;
        }

        /// <summary>
        /// dirty records를 기존 데이터에 머지하여 저장한다.
        /// </summary>
        public void Save(RedDotSaveData data)
        {
            var existing = this.Load();
            MergeInto(existing, data);
            this.WriteData(existing);
        }

        /// <summary>
        /// 전체 데이터를 저장한다. Compact용.
        /// </summary>
        public void SaveAll(RedDotSaveData fullData)
        {
            this.WriteData(fullData);
        }

        public RedDotSaveData Load()
        {
            var json = PlayerPrefs.GetString(this._key, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                return new RedDotSaveData();
            }

            var data = JsonUtility.FromJson<RedDotSaveData>(json);
            if (data == null) return new RedDotSaveData();
            if (data.categories == null) data.categories = new();
            return data;
        }

        public void Clear()
        {
            PlayerPrefs.DeleteKey(this._key);
            PlayerPrefs.Save();
        }

        private void WriteData(RedDotSaveData data)
        {
            var json = JsonUtility.ToJson(data, false);
            PlayerPrefs.SetString(this._key, json);
            PlayerPrefs.Save();
        }

        private static void MergeInto(RedDotSaveData target, RedDotSaveData delta)
        {
            if (delta.categories == null) return;

            foreach (var deltaCat in delta.categories)
            {
                if (deltaCat.records == null) continue;

                RedDotSaveData.CategoryData targetCat = null;
                foreach (var cat in target.categories)
                {
                    if (cat.categoryName == deltaCat.categoryName)
                    {
                        targetCat = cat;
                        break;
                    }
                }

                if (targetCat == null)
                {
                    target.categories.Add(deltaCat);
                    continue;
                }

                foreach (var deltaRec in deltaCat.records)
                {
                    var found = false;
                    for (var i = 0; i < targetCat.records.Count; i++)
                    {
                        if (targetCat.records[i].key == deltaRec.key)
                        {
                            targetCat.records[i] = deltaRec;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        targetCat.records.Add(deltaRec);
                    }
                }
            }
        }
    }
}
