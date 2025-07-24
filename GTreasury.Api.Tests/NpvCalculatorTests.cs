using GTreasury.Api.Functions.Services;
using GTreasury.Api.Utilities.Records;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTreasury.Api.Tests
{
    //Not testing the NPV values as per formula because of time constraints

    [TestClass]
    public class NpvCalculatorTests
    {
        private NpvCalculator _calculator = null!;
        private NpvInput _input = null!;

        [TestInitialize]
        public void Setup()
        {
            _calculator = new NpvCalculator();
            _input = new NpvInput
            {
                LowerRate = 5,
                UpperRate = 7,
                Increment = 1,
                CashFlows =
                [
                    new CashFlow { Year = 1, Amount = 100 },
                    new CashFlow { Year = 2, Amount = 200 },
                    new CashFlow { Year = 3, Amount = 300 }
                ]
            };
        }

        [TestMethod]
        public void Calculate_ReturnsExpectedResults()
        {
            var results = _calculator.Calculate(_input).ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(5, results[0].Rate);
            Assert.AreEqual(6, results[1].Rate);
            Assert.AreEqual(7, results[2].Rate);
            
        }

        [TestMethod]
        public void CalculateParallel_ReturnsExpectedResults()
        {
            var results = _calculator.CalculateParallel(_input).ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(5, results[0].Rate);
            Assert.AreEqual(6, results[1].Rate);
            Assert.AreEqual(7, results[2].Rate);
        }

        [TestMethod]
        public void CalculateBatch_ReturnsExpectedResults()
        {
            var rates = new List<double> { 5, 6, 7 };
            var results = _calculator.CalculateBatch(_input, rates).ToList();

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(5, results[0].Rate);
            Assert.AreEqual(6, results[1].Rate);
            Assert.AreEqual(7, results[2].Rate);
        }

        [TestMethod]
        public void GetRateRange_ReturnsCorrectRange()
        {
            var rates = _calculator.GetRateRange(5, 7, 1).ToList();

            CollectionAssert.AreEqual(new List<double> { 5, 6, 7 }, rates);
        }

        [TestMethod]
        public void Calculate_WithEmptyCashFlows_ReturnsZeroNpv()
        {
            var input = new NpvInput
            {
                LowerRate = 5,
                UpperRate = 5,
                Increment = 1,
                CashFlows = []
            };

            var results = _calculator.Calculate(input).ToList();

            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0, results[0].Value);
        }
    }
}
