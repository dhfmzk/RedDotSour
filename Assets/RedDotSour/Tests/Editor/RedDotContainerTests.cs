using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RedDotSour.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RedDotSour.Tests
{
    [TestFixture]
    public class RedDotContainerTests
    {
        private RedDotContainer<int> _container;

        [SetUp]
        public void SetUp()
        {
            this._container = new RedDotContainer<int>(
                "Test",
                k => k.ToString(),
                s => int.Parse(s));
        }

        #region Register

        [Test]
        public void Register_NewKey_IsOn()
        {
            this._container.Register(1);

            Assert.IsTrue(this._container.IsOn(1));
        }

        [Test]
        public void Register_NewKey_CountOnIncreases()
        {
            this._container.Register(1);
            this._container.Register(2);

            Assert.AreEqual(2, this._container.CountOn());
        }

        [Test]
        public void Register_DuplicateKey_Ignored()
        {
            this._container.Register(1);
            this._container.Register(1);

            Assert.AreEqual(1, this._container.CountOn());
        }

        [Test]
        public void Register_NewKey_IsDirty()
        {
            this._container.Register(1);

            Assert.AreEqual(1, this._container.DirtyCount);
        }

        #endregion

        #region Mark

        [Test]
        public void Mark_RegisteredKey_IsOnFalse()
        {
            this._container.Register(1);
            this._container.Mark(1);

            Assert.IsFalse(this._container.IsOn(1));
        }

        [Test]
        public void Mark_RegisteredKey_CountOnDecreases()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Mark(1);

            Assert.AreEqual(1, this._container.CountOn());
        }

        [Test]
        public void Mark_WithDateTime_StoresValue()
        {
            var at = new DateTime(2026, 4, 12, 12, 0, 0);
            this._container.Register(1);
            this._container.Mark(1, at);

            this._container.TryGet(1, out var record);
            Assert.AreEqual(at, record.CheckedAt);
        }

        [Test]
        public void Mark_UnregisteredKey_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => this._container.Mark(999));
        }

        [Test]
        public void Mark_AlreadyMarkedKey_UpdatesTimestamp()
        {
            var first = new DateTime(2026, 1, 1);
            var second = new DateTime(2026, 6, 1);

            this._container.Register(1);
            this._container.Mark(1, first);
            this._container.Mark(1, second);

            this._container.TryGet(1, out var record);
            Assert.AreEqual(second, record.CheckedAt);
        }

        [Test]
        public void Mark_AlreadyMarkedKey_CountOnUnchanged()
        {
            this._container.Register(1);
            this._container.Mark(1, new DateTime(2026, 1, 1));
            var countBefore = this._container.CountOn();

            this._container.Mark(1, new DateTime(2026, 6, 1));

            Assert.AreEqual(countBefore, this._container.CountOn());
        }

        #endregion

        #region Unmark

        [Test]
        public void Unmark_MarkedKey_IsOnTrue()
        {
            this._container.Register(1);
            this._container.Mark(1);
            this._container.Unmark(1);

            Assert.IsTrue(this._container.IsOn(1));
        }

        [Test]
        public void Unmark_MarkedKey_CountOnIncreases()
        {
            this._container.Register(1);
            this._container.Mark(1);
            this._container.Unmark(1);

            Assert.AreEqual(1, this._container.CountOn());
        }

        [Test]
        public void Unmark_AlreadyUnmarkedKey_NoChange()
        {
            this._container.Register(1);
            var countBefore = this._container.CountOn();

            this._container.Unmark(1);

            Assert.AreEqual(countBefore, this._container.CountOn());
        }

        [Test]
        public void Unmark_UnregisteredKey_ThrowsKeyNotFoundException()
        {
            Assert.Throws<KeyNotFoundException>(() => this._container.Unmark(999));
        }

        #endregion

        #region Remove

        [Test]
        public void Remove_ExistingKey_ReturnsTrue()
        {
            this._container.Register(1);

            Assert.IsTrue(this._container.Remove(1));
        }

        [Test]
        public void Remove_NonExistingKey_ReturnsFalse()
        {
            Assert.IsFalse(this._container.Remove(999));
        }

        [Test]
        public void Remove_OnKey_CountOnDecreases()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Remove(1);

            Assert.AreEqual(1, this._container.CountOn());
        }

        [Test]
        public void Remove_MarkedKey_CountOnUnchanged()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Mark(1);
            var countBefore = this._container.CountOn();

            this._container.Remove(1);

            Assert.AreEqual(countBefore, this._container.CountOn());
        }

        [Test]
        public void Remove_DirtyKey_DirtyCountDecreases()
        {
            this._container.Register(1);
            this._container.Register(2);
            var dirtyBefore = this._container.DirtyCount;

            this._container.Remove(1);

            Assert.AreEqual(dirtyBefore - 1, this._container.DirtyCount);
        }

        #endregion

        #region IsOn / TryGet

        [Test]
        public void IsOn_UnregisteredKey_ReturnsFalse()
        {
            Assert.IsFalse(this._container.IsOn(999));
        }

        [Test]
        public void TryGet_RegisteredKey_ReturnsTrue()
        {
            this._container.Register(1);

            Assert.IsTrue(this._container.TryGet(1, out _));
        }

        [Test]
        public void TryGet_UnregisteredKey_ReturnsFalse()
        {
            Assert.IsFalse(this._container.TryGet(999, out _));
        }

        [Test]
        public void TryGet_RegisteredKey_RecordIsOnTrue()
        {
            this._container.Register(1);

            this._container.TryGet(1, out var record);
            Assert.IsTrue(record.IsOn);
        }

        [Test]
        public void TryGet_MarkedKey_RecordIsOnFalse()
        {
            this._container.Register(1);
            this._container.Mark(1);

            this._container.TryGet(1, out var record);
            Assert.IsFalse(record.IsOn);
        }

        #endregion

        #region IsOnAny / CountOn

        [Test]
        public void IsOnAny_Empty_ReturnsFalse()
        {
            Assert.IsFalse(this._container.IsOnAny());
        }

        [Test]
        public void IsOnAny_AllMarked_ReturnsFalse()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Mark(1);
            this._container.Mark(2);

            Assert.IsFalse(this._container.IsOnAny());
        }

        [Test]
        public void CountOn_Empty_ReturnsZero()
        {
            Assert.AreEqual(0, this._container.CountOn());
        }

        [Test]
        public void CountOn_MixedState_ReturnsCorrectCount()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Register(3);
            this._container.Mark(2);

            Assert.AreEqual(2, this._container.CountOn());
        }

        #endregion

        #region OnChanged Event

        [Test]
        public void Register_FiresOnChanged()
        {
            var fired = false;
            this._container.OnChanged += () => fired = true;

            this._container.Register(1);

            Assert.IsTrue(fired);
        }

        [Test]
        public void Mark_FiresOnChanged()
        {
            this._container.Register(1);

            var fired = false;
            this._container.OnChanged += () => fired = true;
            this._container.Mark(1);

            Assert.IsTrue(fired);
        }

        [Test]
        public void Unmark_MarkedKey_FiresOnChanged()
        {
            this._container.Register(1);
            this._container.Mark(1);

            var fired = false;
            this._container.OnChanged += () => fired = true;
            this._container.Unmark(1);

            Assert.IsTrue(fired);
        }

        [Test]
        public void Unmark_AlreadyUnmarkedKey_DoesNotFire()
        {
            this._container.Register(1);

            var fired = false;
            this._container.OnChanged += () => fired = true;
            this._container.Unmark(1);

            Assert.IsFalse(fired);
        }

        [Test]
        public void OnChanged_SubscriberException_DoesNotBlockOthers()
        {
            var secondFired = false;
            this._container.OnChanged += () => throw new Exception("boom");
            this._container.OnChanged += () => secondFired = true;

            LogAssert.Expect(LogType.Exception, "Exception: boom");
            this._container.Register(1);

            Assert.IsTrue(secondFired);
        }

        #endregion

        #region Dirty Tracking

        [Test]
        public void DirtyCount_AfterRegisterAndMark_CountsBoth()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Mark(1);

            Assert.AreEqual(2, this._container.DirtyCount);
        }

        [Test]
        public void ClearDirty_ResetsDirtyCount()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.ClearDirty();

            Assert.AreEqual(0, this._container.DirtyCount);
        }

        [Test]
        public void GetDirtyRecords_ReturnsOnlyDirty()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.ClearDirty();

            this._container.Mark(1);

            var dirty = this._container.GetDirtyRecords().ToList();
            Assert.AreEqual(1, dirty.Count);
            Assert.AreEqual(1, dirty[0].Key);
        }

        [Test]
        public void GetAllRecords_ReturnsAll()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Register(3);

            var all = this._container.GetAllRecords().ToList();
            Assert.AreEqual(3, all.Count);
        }

        #endregion

        #region LoadRecord

        [Test]
        public void LoadRecord_NullCheckedAt_IsOn()
        {
            this._container.LoadRecord(1, null);

            Assert.IsTrue(this._container.IsOn(1));
            Assert.AreEqual(1, this._container.CountOn());
        }

        [Test]
        public void LoadRecord_WithCheckedAt_IsNotOn()
        {
            this._container.LoadRecord(1, new DateTime(2026, 4, 12));

            Assert.IsFalse(this._container.IsOn(1));
            Assert.AreEqual(0, this._container.CountOn());
        }

        [Test]
        public void LoadRecord_NotDirty()
        {
            this._container.LoadRecord(1, null);

            Assert.AreEqual(0, this._container.DirtyCount);
        }

        #endregion

        #region ClearAll

        [Test]
        public void ClearAll_ResetsEverything()
        {
            this._container.Register(1);
            this._container.Register(2);
            this._container.Mark(1);

            this._container.ClearAll();

            Assert.AreEqual(0, this._container.CountOn());
            Assert.AreEqual(0, this._container.DirtyCount);
            Assert.IsFalse(this._container.IsOnAny());
        }

        [Test]
        public void ClearAll_FiresOnChanged()
        {
            this._container.Register(1);

            var fired = false;
            this._container.OnChanged += () => fired = true;
            this._container.ClearAll();

            Assert.IsTrue(fired);
        }

        #endregion
    }
}
