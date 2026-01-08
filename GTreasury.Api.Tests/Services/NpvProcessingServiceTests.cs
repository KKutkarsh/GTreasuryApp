using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTreasury.Api.Functions.Services;
using GTreasury.Api.Functions.Services.Interface;
using GTreasury.Api.Utilities.Dtos;
using GTreasury.Api.Utilities.Records;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace GTreasury.Api.Tests.Services
{
    [TestClass]
    public class NpvProcessingServiceTests
    {
        private Mock<INpvCalculator> _calculatorMock = null!;
        private Mock<IFeatureManager> _featureManagerMock = null!;
        private Mock<ILogger<NpvProcessingService>> _loggerMock = null!;
        private IOptions<NpvSettings> _options = null!;
        private NpvProcessingService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _calculatorMock = new Mock<INpvCalculator>(MockBehavior.Strict);
            _featureManagerMock = new Mock<IFeatureManager>(MockBehavior.Strict);
            _loggerMock = new Mock<ILogger<NpvProcessingService>>();

            _options = Options.Create(new NpvSettings
            {
                MaxBatchSize = 2
            });

            _service = new NpvProcessingService(
                _calculatorMock.Object,
                _featureManagerMock.Object,
                _options,
                _loggerMock.Object);
        }

        [TestMethod]
        public async Task ProcessAsync_WhenBatchFeatureDisabled_CallsCalculateAndReturnsResult()
        {
            // Arrange
            var input = CreateSampleInput();

            _featureManagerMock
                .Setup(m => m.IsEnabledAsync("EnableNpvBatchProcessing"))
                .ReturnsAsync(false);

            var expectedResults = new List<NpvResult>
            {
                new NpvResult { Rate = 1.0, Value = 100.0 }
            };

            _calculatorMock
                .Setup(c => c.Calculate(input))
                .Returns(expectedResults);

            // Act
            var resultObj = await _service.ProcessAsync(input);

            // Assert
            _featureManagerMock.Verify(m => m.IsEnabledAsync("EnableNpvBatchProcessing"), Times.Once);
            _calculatorMock.Verify(c => c.Calculate(input), Times.Once);
            _calculatorMock.Verify(c => c.GetRateRange(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>()), Times.Never);
            _calculatorMock.Verify(c => c.CalculateBatch(It.IsAny<NpvInput>(), It.IsAny<IEnumerable<double>>()), Times.Never);

            var result = resultObj as IEnumerable<NpvResult>;
            Assert.IsNotNull(result, "Result should be castable to IEnumerable<NpvResult>.");
            var list = result.ToList();
            Assert.AreEqual(1, list.Count, "Result should contain exactly one item.");
            Assert.AreEqual(1.0, list[0].Rate);
            Assert.AreEqual(100.0, list[0].Value);
        }

        [TestMethod]
        public async Task ProcessAsync_WhenBatchFeatureEnabled_UsesGetRateRangeAndCalculateBatchAndReturnsFlattenedResults()
        {
            // Arrange
            var input = new NpvInput
            {
                CashFlows =
                [
                    new CashFlow { Year = 2024, Amount = -1000 },
                    new CashFlow { Year = 2025, Amount = 600 },
                    new CashFlow { Year = 2026, Amount = 600 }
                ],
                LowerRate = 1.0,
                UpperRate = 4.0,
                Increment = 1.0,
                EvaluationYear = 2024
            };

            _featureManagerMock
                .Setup(m => m.IsEnabledAsync("EnableNpvBatchProcessing"))
                .ReturnsAsync(true);

            var rates = new[] { 1.0, 2.0, 3.0, 4.0 };

            _calculatorMock
                .Setup(c => c.GetRateRange(input.LowerRate, input.UpperRate, input.Increment))
                .Returns(rates);

            _calculatorMock
                .Setup(c => c.CalculateBatch(input,
                    It.Is<IEnumerable<double>>(r => r.SequenceEqual(new[] { 1.0, 2.0 }))))

                .Returns(new[]
                {
                    new NpvResult { Rate = 1.0, Value = 10.0 },
                    new NpvResult { Rate = 2.0, Value = 20.0 }
                });

            _calculatorMock
                .Setup(c => c.CalculateBatch(input,
                    It.Is<IEnumerable<double>>(r => r.SequenceEqual(new[] { 3.0, 4.0 }))))

                .Returns(new[]
                {
                    new NpvResult { Rate = 3.0, Value = 30.0 },
                    new NpvResult { Rate = 4.0, Value = 40.0 }
                });

            // No single Calculate call expected in batch mode
            _calculatorMock
                .Setup(c => c.Calculate(It.IsAny<NpvInput>()))
                .Throws(new InvalidOperationException("Calculate should not be called in batch mode."));

            // Act
            var resultObj = await _service.ProcessAsync(input);

            // Assert
            _featureManagerMock.Verify(m => m.IsEnabledAsync("EnableNpvBatchProcessing"), Times.Once);
            _calculatorMock.Verify(c => c.GetRateRange(input.LowerRate, input.UpperRate, input.Increment), Times.Once);

            _calculatorMock.Verify(c => c.CalculateBatch(
                input,
                It.Is<IEnumerable<double>>(r => r.SequenceEqual(new[] { 1.0, 2.0 }))), Times.Once);
            _calculatorMock.Verify(c => c.CalculateBatch(
                input,
                It.Is<IEnumerable<double>>(r => r.SequenceEqual(new[] { 3.0, 4.0 }))), Times.Once);

            var resultList = resultObj as List<NpvResult>;
            Assert.IsNotNull(resultList, "Result should be castable to List<NpvResult>.");
            Assert.AreEqual(4, resultList.Count, "Result should contain four items.");

            Assert.AreEqual(1.0, resultList[0].Rate);
            Assert.AreEqual(10.0, resultList[0].Value);
            Assert.AreEqual(2.0, resultList[1].Rate);
            Assert.AreEqual(20.0, resultList[1].Value);
            Assert.AreEqual(3.0, resultList[2].Rate);
            Assert.AreEqual(30.0, resultList[2].Value);
            Assert.AreEqual(4.0, resultList[3].Rate);
            Assert.AreEqual(40.0, resultList[3].Value);
        }

        [TestMethod]
        public async Task ProcessAsync_WhenBatchFeatureEnabled_RespectsMaxBatchSize()
        {
            // Arrange
            var input = new NpvInput
            {
                CashFlows =
                [
                    new CashFlow { Year = 2024, Amount = -1000 },
                    new CashFlow { Year = 2025, Amount = 600 },
                    new CashFlow { Year = 2026, Amount = 600 }
                ],
                LowerRate = 1.0,
                UpperRate = 5.0,
                Increment = 1.0,
                EvaluationYear = 2024
            };

            // MaxBatchSize is 2 (from Setup)
            _featureManagerMock
                .Setup(m => m.IsEnabledAsync("EnableNpvBatchProcessing"))
                .ReturnsAsync(true);

            var rates = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };
            _calculatorMock
                .Setup(c => c.GetRateRange(input.LowerRate, input.UpperRate, input.Increment))
                .Returns(rates);

            var capturedChunkSizes = new List<int>();

            _calculatorMock
                .Setup(c => c.CalculateBatch(input, It.IsAny<IEnumerable<double>>()))
                .Returns<NpvInput, IEnumerable<double>>((_, chunk) =>
                {
                    var chunkList = chunk.ToList();
                    capturedChunkSizes.Add(chunkList.Count);
                    // Return dummy results for each rate in the chunk
                    return chunkList.Select(r => new NpvResult { Rate = r, Value = r * 10 });
                });

            _calculatorMock
                .Setup(c => c.Calculate(It.IsAny<NpvInput>()))
                .Throws(new InvalidOperationException("Calculate should not be called in batch mode."));

            // Act
            var resultObj = await _service.ProcessAsync(input);

            // Assert
            // Expect 3 chunks with MaxBatchSize=2: [1,2], [3,4], [5]
            Assert.AreEqual(3, capturedChunkSizes.Count, "Expected three chunks.");
            Assert.AreEqual(2, capturedChunkSizes[0], "First chunk should have 2 elements.");
            Assert.AreEqual(2, capturedChunkSizes[1], "Second chunk should have 2 elements.");
            Assert.AreEqual(1, capturedChunkSizes[2], "Third chunk should have 1 element.");

            var results = resultObj as List<NpvResult>;
            Assert.IsNotNull(results, "Result should be castable to List<NpvResult>.");
            Assert.AreEqual(5, results.Count, "Should get one result per rate.");
        }

        private static NpvInput CreateSampleInput()
        {
            return new NpvInput
            {
                CashFlows =
                [
                    new CashFlow { Year = 2024, Amount = -1000 },
                    new CashFlow { Year = 2025, Amount = 600 },
                    new CashFlow { Year = 2026, Amount = 600 }
                ],
                LowerRate = 1.0,
                UpperRate = 5.0,
                Increment = 1.0,
                EvaluationYear = 2024
            };
        }
    }
}