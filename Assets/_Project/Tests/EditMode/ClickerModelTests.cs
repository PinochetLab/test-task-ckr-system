using NUnit.Framework;
using TestTask.Features.Clicker.Model;

namespace TestTask.Tests
{
    public sealed class ClickerModelTests
    {
        [Test]
        public void TrySpendEnergy_NotEnough_ReturnsFalseAndKeepsEnergy()
        {
            var model = new ClickerModel(maxEnergy: 10, startEnergy: 2);

            Assert.IsFalse(model.TrySpendEnergy(3));
            Assert.AreEqual(2, model.Energy.Value);
        }

        [Test]
        public void TrySpendEnergy_Enough_Decreases()
        {
            var model = new ClickerModel(10, 10);

            Assert.IsTrue(model.TrySpendEnergy(4));
            Assert.AreEqual(6, model.Energy.Value);
        }

        [Test]
        public void RestoreEnergy_IsClampedByMax()
        {
            var model = new ClickerModel(10, 8);

            model.RestoreEnergy(5);

            Assert.AreEqual(10, model.Energy.Value);
        }

        [Test]
        public void AddCurrency_Accumulates()
        {
            var model = new ClickerModel(10, 10);

            model.AddCurrency(3);
            model.AddCurrency(2);

            Assert.AreEqual(5, model.Currency.Value);
        }

        [Test]
        public void StartEnergy_IsClampedByMax()
        {
            var model = new ClickerModel(10, 50);

            Assert.AreEqual(10, model.Energy.Value);
        }
    }
}
