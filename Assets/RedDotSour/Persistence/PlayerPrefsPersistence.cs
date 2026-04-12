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

        public void Save(RedDotSaveData data)
        {
            this.WriteData(data);
        }

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

            return JsonUtility.FromJson<RedDotSaveData>(json) ?? new RedDotSaveData();
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
    }
}
