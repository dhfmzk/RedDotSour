using System.IO;
using System.Text;
using UnityEngine;

namespace RedDotSour.Persistence
{
    public class JournalFilePersistence : IRedDotPersistence
    {
        private readonly string _snapshotPath;
        private readonly string _journalPath;

        public JournalFilePersistence(string baseName = "reddotsour")
        {
            var dir = Application.persistentDataPath;
            this._snapshotPath = Path.Combine(dir, baseName + ".snapshot.json");
            this._journalPath = Path.Combine(dir, baseName + ".journal");
        }

        /// <summary>
        /// dirty records만 journal에 append한다.
        /// </summary>
        public void Save(RedDotSaveData data)
        {
            var sb = new StringBuilder();
            foreach (var cat in data.categories)
            {
                foreach (var rec in cat.records)
                {
                    var entry = new JournalEntry
                    {
                        cat = cat.categoryName,
                        key = rec.key,
                        ticks = rec.checkedAtTicks
                    };
                    sb.AppendLine(JsonUtility.ToJson(entry));
                }
            }

            if (sb.Length > 0)
            {
                File.AppendAllText(this._journalPath, sb.ToString());
            }
        }

        /// <summary>
        /// 전체 데이터를 snapshot으로 저장하고 journal을 삭제한다.
        /// </summary>
        public void SaveAll(RedDotSaveData fullData)
        {
            var json = JsonUtility.ToJson(fullData, false);

            // atomic write: temp → rename
            var tempPath = this._snapshotPath + ".tmp";
            File.WriteAllText(tempPath, json);

            if (File.Exists(this._snapshotPath))
            {
                File.Delete(this._snapshotPath);
            }

            File.Move(tempPath, this._snapshotPath);

            // journal 삭제
            if (File.Exists(this._journalPath))
            {
                File.Delete(this._journalPath);
            }
        }

        /// <summary>
        /// snapshot을 로드하고 journal을 재생한다.
        /// </summary>
        public RedDotSaveData Load()
        {
            var data = this.LoadSnapshot();
            this.ReplayJournal(data);
            return data;
        }

        /// <summary>
        /// snapshot과 journal을 모두 삭제한다.
        /// </summary>
        public void Clear()
        {
            if (File.Exists(this._snapshotPath)) File.Delete(this._snapshotPath);
            if (File.Exists(this._journalPath)) File.Delete(this._journalPath);
        }

        private RedDotSaveData LoadSnapshot()
        {
            if (!File.Exists(this._snapshotPath))
            {
                return new RedDotSaveData();
            }

            var json = File.ReadAllText(this._snapshotPath);
            if (string.IsNullOrEmpty(json))
            {
                return new RedDotSaveData();
            }

            return JsonUtility.FromJson<RedDotSaveData>(json) ?? new RedDotSaveData();
        }

        private void ReplayJournal(RedDotSaveData data)
        {
            if (!File.Exists(this._journalPath))
            {
                return;
            }

            var lines = File.ReadAllLines(this._journalPath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                JournalEntry entry;
                try
                {
                    entry = JsonUtility.FromJson<JournalEntry>(line);
                }
                catch
                {
                    // 깨진 줄은 skip (크래시 안전성)
                    continue;
                }

                this.ApplyJournalEntry(data, entry);
            }
        }

        private void ApplyJournalEntry(RedDotSaveData data, JournalEntry entry)
        {
            // 해당 카테고리 찾기 또는 생성
            RedDotSaveData.CategoryData catData = null;
            foreach (var cat in data.categories)
            {
                if (cat.categoryName == entry.cat)
                {
                    catData = cat;
                    break;
                }
            }

            if (catData == null)
            {
                catData = new RedDotSaveData.CategoryData
                {
                    categoryName = entry.cat
                };
                data.categories.Add(catData);
            }

            // 해당 키 찾기 또는 추가
            for (var i = 0; i < catData.records.Count; i++)
            {
                if (catData.records[i].key == entry.key)
                {
                    catData.records[i] = new RedDotSaveData.RecordData
                    {
                        key = entry.key,
                        checkedAtTicks = entry.ticks
                    };
                    return;
                }
            }

            catData.records.Add(new RedDotSaveData.RecordData
            {
                key = entry.key,
                checkedAtTicks = entry.ticks
            });
        }
    }
}
