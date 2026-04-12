using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RedDotSour.Core;
using RedDotSour.Persistence;

namespace RedDotSour.Tests
{
    /// <summary>
    /// IRedDotPersistence의 인메모리 구현. 테스트 전용.
    /// </summary>
    public class MockPersistence : IRedDotPersistence
    {
        public RedDotSaveData LastSaved { get; private set; }
        public RedDotSaveData LastSavedAll { get; private set; }
        public RedDotSaveData DataToLoad { get; set; }
        public int SaveCallCount { get; private set; }
        public int SaveAllCallCount { get; private set; }
        public bool Cleared { get; private set; }

        public void Save(RedDotSaveData data)
        {
            this.LastSaved = data;
            this.SaveCallCount++;
        }

        public void SaveAll(RedDotSaveData fullData)
        {
            this.LastSavedAll = fullData;
            this.SaveAllCallCount++;
        }

        public RedDotSaveData Load()
        {
            return this.DataToLoad ?? new RedDotSaveData();
        }

        public void Clear()
        {
            this.Cleared = true;
        }
    }

    public enum PersistTestCategory
    {
        Inventory,
        Quest,
    }

    [TestFixture]
    public class PersistenceTests
    {
        private RedDotSour<PersistTestCategory> _redDot;
        private MockPersistence _mock;

        [SetUp]
        public void SetUp()
        {
            this._redDot = new RedDotSour<PersistTestCategory>();
            this._mock = new MockPersistence();
            this._redDot.SetPersistence(this._mock);
        }

        #region Export / Import

        [Test]
        public void ExportDirtyRecords_ReturnsOnlyDirty()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);
            inv.Register(2);
            inv.ClearDirty();
            inv.Mark(1);

            var records = inv.ExportDirtyRecords();

            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("1", records[0].key);
            Assert.AreNotEqual(0, records[0].checkedAtTicks);
        }

        [Test]
        public void ExportAllRecords_ReturnsAll()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);
            inv.Register(2);
            inv.Mark(1);

            var records = inv.ExportAllRecords();

            Assert.AreEqual(2, records.Count);
        }

        [Test]
        public void ImportRecords_RestoresState()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            var records = new List<RedDotSaveData.RecordData>
            {
                new() { key = "10", checkedAtTicks = 0 },
                new() { key = "20", checkedAtTicks = new DateTime(2026, 4, 12).Ticks },
            };

            inv.ImportRecords(records);

            Assert.IsTrue(inv.IsOn(10));
            Assert.IsFalse(inv.IsOn(20));
            Assert.AreEqual(1, inv.CountOn());
        }

        [Test]
        public void ImportRecords_CheckedAtTicks_Zero_IsNull()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            inv.ImportRecords(new List<RedDotSaveData.RecordData>
            {
                new() { key = "1", checkedAtTicks = 0 }
            });

            inv.TryGet(1, out var record);
            Assert.IsTrue(record.IsOn);
            Assert.IsNull(record.CheckedAt);
        }

        #endregion

        #region Save

        [Test]
        public void Save_CallsPersistenceWithDirtyOnly()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);
            inv.Register(2);
            inv.ClearDirty();
            inv.Mark(1);

            this._redDot.Save();

            Assert.AreEqual(1, this._mock.SaveCallCount);
            Assert.AreEqual(1, this._mock.LastSaved.categories.Count);
            Assert.AreEqual(1, this._mock.LastSaved.categories[0].records.Count);
        }

        [Test]
        public void Save_ClearsDirtyAfterSave()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);

            this._redDot.Save();

            Assert.AreEqual(0, inv.DirtyCount);
        }

        [Test]
        public void Save_NoDirty_SkipsEmptyCategories()
        {
            this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            this._redDot.Save();

            Assert.AreEqual(0, this._mock.LastSaved.categories.Count);
        }

        [Test]
        public void Save_NoPersistence_DoesNotThrow()
        {
            var redDot = new RedDotSour<PersistTestCategory>();
            var inv = redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);

            Assert.DoesNotThrow(() => redDot.Save());
        }

        #endregion

        #region Compact

        [Test]
        public void Compact_CallsSaveAllWithAllRecords()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);
            inv.Register(2);
            inv.Mark(1);

            this._redDot.Compact();

            Assert.AreEqual(1, this._mock.SaveAllCallCount);
            Assert.AreEqual(2, this._mock.LastSavedAll.categories[0].records.Count);
        }

        [Test]
        public void Compact_ClearsDirty()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);

            this._redDot.Compact();

            Assert.AreEqual(0, inv.DirtyCount);
        }

        #endregion

        #region Load

        [Test]
        public void Load_RestoresFromPersistence()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            this._mock.DataToLoad = new RedDotSaveData
            {
                categories = new()
                {
                    new RedDotSaveData.CategoryData
                    {
                        categoryName = "Inventory",
                        records = new()
                        {
                            new() { key = "100", checkedAtTicks = 0 },
                            new() { key = "200", checkedAtTicks = new DateTime(2026, 1, 1).Ticks },
                        }
                    }
                }
            };

            this._redDot.Load();

            Assert.IsTrue(inv.IsOn(100));
            Assert.IsFalse(inv.IsOn(200));
            Assert.AreEqual(1, inv.CountOn());
        }

        [Test]
        public void Load_UnknownCategory_Ignored()
        {
            this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            this._mock.DataToLoad = new RedDotSaveData
            {
                categories = new()
                {
                    new RedDotSaveData.CategoryData
                    {
                        categoryName = "NonExistent",
                        records = new()
                        {
                            new() { key = "1", checkedAtTicks = 0 }
                        }
                    }
                }
            };

            Assert.DoesNotThrow(() => this._redDot.Load());
        }

        [Test]
        public void Load_NoPersistence_DoesNotThrow()
        {
            var redDot = new RedDotSour<PersistTestCategory>();
            Assert.DoesNotThrow(() => redDot.Load());
        }

        #endregion

        #region Round Trip

        [Test]
        public void SaveThenLoad_RoundTrip_DataIntact()
        {
            // Setup: 데이터 생성
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            inv.Register(1);
            inv.Register(2);
            inv.Register(3);
            inv.Mark(2, new DateTime(2026, 4, 12));

            // Compact로 전체 저장
            this._redDot.Compact();
            var savedData = this._mock.LastSavedAll;

            // 새 인스턴스에서 로드
            var redDot2 = new RedDotSour<PersistTestCategory>();
            var mock2 = new MockPersistence { DataToLoad = savedData };
            redDot2.SetPersistence(mock2);
            var inv2 = redDot2.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            redDot2.Load();

            Assert.IsTrue(inv2.IsOn(1));
            Assert.IsFalse(inv2.IsOn(2));
            Assert.IsTrue(inv2.IsOn(3));
            Assert.AreEqual(2, inv2.CountOn());
        }

        [Test]
        public void MultipleCategories_SaveLoad_AllPreserved()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            var quest = this._redDot.Create<long>(
                PersistTestCategory.Quest, k => k.ToString(), long.Parse);
            inv.Register(1);
            quest.Register(100L);
            quest.Mark(100L);

            this._redDot.Compact();
            var savedData = this._mock.LastSavedAll;

            // 새 인스턴스에서 로드
            var redDot2 = new RedDotSour<PersistTestCategory>();
            var mock2 = new MockPersistence { DataToLoad = savedData };
            redDot2.SetPersistence(mock2);
            var inv2 = redDot2.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);
            var quest2 = redDot2.Create<long>(
                PersistTestCategory.Quest, k => k.ToString(), long.Parse);
            redDot2.Load();

            Assert.IsTrue(inv2.IsOn(1));
            Assert.IsFalse(quest2.IsOn(100L));
        }

        #endregion

        #region CategoryName

        [Test]
        public void Container_CategoryName_MatchesEnum()
        {
            var inv = this._redDot.Create<int>(
                PersistTestCategory.Inventory, k => k.ToString(), int.Parse);

            Assert.AreEqual("Inventory", inv.CategoryName);
        }

        #endregion
    }
}
