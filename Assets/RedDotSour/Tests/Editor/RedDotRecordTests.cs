using System;
using NUnit.Framework;
using RedDotSour.Core;

namespace RedDotSour.Tests
{
    [TestFixture]
    public class RedDotRecordTests
    {
        [Test]
        public void IsOn_NullCheckedAt_ReturnsTrue()
        {
            var record = new RedDotRecord<int>(1, null);

            Assert.IsTrue(record.IsOn);
        }

        [Test]
        public void IsOn_WithCheckedAt_ReturnsFalse()
        {
            var record = new RedDotRecord<int>(1, new DateTime(2026, 4, 12));

            Assert.IsFalse(record.IsOn);
        }

        [Test]
        public void Key_StoresCorrectValue()
        {
            var record = new RedDotRecord<int>(42, null);

            Assert.AreEqual(42, record.Key);
        }

        [Test]
        public void CheckedAt_StoresCorrectValue()
        {
            var at = new DateTime(2026, 4, 12, 15, 30, 0);
            var record = new RedDotRecord<int>(1, at);

            Assert.AreEqual(at, record.CheckedAt);
        }

        [Test]
        public void Default_IsNotOn()
        {
            var record = default(RedDotRecord<int>);

            Assert.IsTrue(record.IsOn);
            Assert.AreEqual(0, record.Key);
            Assert.IsNull(record.CheckedAt);
        }
    }
}
