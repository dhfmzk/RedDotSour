using System;
using NUnit.Framework;
using RedDotSour.Core;

namespace RedDotSour.Tests
{
    public enum TestCategory
    {
        Inventory,
        Quest,
        Mail,
    }

    [TestFixture]
    public class RedDotSourTests
    {
        private RedDotSour<TestCategory> _redDot;

        [SetUp]
        public void SetUp()
        {
            this._redDot = new RedDotSour<TestCategory>();
        }

        #region Create

        [Test]
        public void Create_ReturnsContainer()
        {
            var container = this._redDot.Create<int>(TestCategory.Inventory);

            Assert.IsNotNull(container);
        }

        [Test]
        public void Create_DuplicateCategory_ThrowsInvalidOperationException()
        {
            this._redDot.Create<int>(TestCategory.Inventory);

            Assert.Throws<InvalidOperationException>(() =>
                this._redDot.Create<int>(TestCategory.Inventory));
        }

        [Test]
        public void Create_DifferentCategories_DifferentKeyTypes()
        {
            var intContainer = this._redDot.Create<int>(TestCategory.Inventory);
            var longContainer = this._redDot.Create<long>(TestCategory.Quest);

            Assert.IsNotNull(intContainer);
            Assert.IsNotNull(longContainer);
        }

        #endregion

        #region Get

        [Test]
        public void Get_ExistingCategory_ReturnsContainer()
        {
            this._redDot.Create<int>(TestCategory.Inventory);

            var container = this._redDot.Get<int>(TestCategory.Inventory);

            Assert.IsNotNull(container);
        }

        [Test]
        public void Get_NonExistingCategory_ReturnsNull()
        {
            var container = this._redDot.Get<int>(TestCategory.Inventory);

            Assert.IsNull(container);
        }

        [Test]
        public void Get_TypeMismatch_ThrowsInvalidCastException()
        {
            this._redDot.Create<int>(TestCategory.Inventory);

            Assert.Throws<InvalidCastException>(() =>
                this._redDot.Get<long>(TestCategory.Inventory));
        }

        [Test]
        public void Get_ReturnsSameInstance()
        {
            var created = this._redDot.Create<int>(TestCategory.Inventory);
            var got = this._redDot.Get<int>(TestCategory.Inventory);

            Assert.AreSame(created, got);
        }

        #endregion

        #region IsOn

        [Test]
        public void IsOn_RegisteredKey_ReturnsTrue()
        {
            var container = this._redDot.Create<int>(TestCategory.Inventory);
            container.Register(1);

            Assert.IsTrue(this._redDot.IsOn(TestCategory.Inventory, 1));
        }

        [Test]
        public void IsOn_MarkedKey_ReturnsFalse()
        {
            var container = this._redDot.Create<int>(TestCategory.Inventory);
            container.Register(1);
            container.Mark(1);

            Assert.IsFalse(this._redDot.IsOn(TestCategory.Inventory, 1));
        }

        [Test]
        public void IsOn_NonExistingCategory_ReturnsFalse()
        {
            Assert.IsFalse(this._redDot.IsOn(TestCategory.Inventory, 1));
        }

        #endregion

        #region IsOnAny / CountOn

        [Test]
        public void IsOnAny_WithUnmarkedKeys_ReturnsTrue()
        {
            var container = this._redDot.Create<int>(TestCategory.Inventory);
            container.Register(1);

            Assert.IsTrue(this._redDot.IsOnAny(TestCategory.Inventory));
        }

        [Test]
        public void IsOnAny_NonExistingCategory_ReturnsFalse()
        {
            Assert.IsFalse(this._redDot.IsOnAny(TestCategory.Inventory));
        }

        [Test]
        public void CountOn_ReturnsContainerCount()
        {
            var container = this._redDot.Create<int>(TestCategory.Inventory);
            container.Register(1);
            container.Register(2);
            container.Register(3);
            container.Mark(1);

            Assert.AreEqual(2, this._redDot.CountOn(TestCategory.Inventory));
        }

        [Test]
        public void CountOn_NonExistingCategory_ReturnsZero()
        {
            Assert.AreEqual(0, this._redDot.CountOn(TestCategory.Inventory));
        }

        #endregion
    }
}
