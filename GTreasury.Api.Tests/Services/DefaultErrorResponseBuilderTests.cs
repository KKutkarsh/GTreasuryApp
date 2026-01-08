using FluentValidation;
using FluentValidation.Results;
using GTreasury.Api.Functions.Services;
using GTreasury.Api.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTreasury.Api.Tests.Services
{
    [TestClass]
    public class DefaultErrorResponseBuilderTests
    {
        private DefaultErrorResponseBuilder _builder = null!;

        [TestInitialize]
        public void Setup()
        {
            _builder = new DefaultErrorResponseBuilder();
        }

        [TestMethod]
        public void Build_WithNonValidationException_DoesNotAddErrorsDetail()
        {
            // Arrange
            var ex = new Exception("some error");

            // Act
            var response = _builder.Build(ex);

            // Assert
            Assert.IsNotNull(response, "Response should not be null.");
            Assert.IsNotNull(response.Details, "Details dictionary should be initialized.");

            // We only assert that no validation 'errors' key is added.
            Assert.IsFalse(response.Details.ContainsKey("errors"),
                "Details should not contain 'errors' key for non-validation exceptions.");
        }

        [TestMethod]
        public void Build_WithValidationException_SetsMessageToValidationFailureConstantAndAddsErrorsKey()
        {
            // Arrange
            var failures = new[]
            {
                new ValidationFailure("LowerRate", "must be greater than zero")
            };
            var validationException = new ValidationException(failures);

            // Act
            var response = _builder.Build(validationException);

            // Assert
            Assert.IsNotNull(response, "Response should not be null.");
            Assert.AreEqual(AppConstants.ExceptionMessages.ValidationFailure, response.Message,
                "Message should be set to the standard validation failure message.");

            Assert.IsNotNull(response.Details, "Details dictionary should be initialized.");
            Assert.IsTrue(response.Details.ContainsKey("errors"),
                "Details should contain 'errors' key for validation exceptions.");
        }

        [TestMethod]
        public void Build_WithValidationException_GroupsErrorsByPropertyName()
        {
            // Arrange
            var failures = new[]
            {
                new ValidationFailure("LowerRate", "must be >= 0"),
                new ValidationFailure("LowerRate", "must be less than UpperRate"),
                new ValidationFailure("UpperRate", "must be <= 100")
            };

            var validationException = new ValidationException(failures);

            // Act
            var response = _builder.Build(validationException);

            // Assert
            Assert.IsNotNull(response.Details, "Details dictionary should be initialized.");
            Assert.IsTrue(response.Details.TryGetValue("errors", out var errorsObj),
                "Details should contain 'errors' key.");

            Assert.IsInstanceOfType(errorsObj, typeof(Dictionary<string, string[]>),
                "'errors' value should be a Dictionary<string, string[]>.");

            var errors = errorsObj as Dictionary<string, string[]>;
            Assert.IsNotNull(errors, "Errors dictionary should not be null.");

            Assert.IsTrue(errors.ContainsKey("LowerRate"), "Errors should contain 'LowerRate' key.");
            Assert.IsTrue(errors.ContainsKey("UpperRate"), "Errors should contain 'UpperRate' key.");

            var lowerRateErrors = errors["LowerRate"];
            var upperRateErrors = errors["UpperRate"];

            Assert.AreEqual(2, lowerRateErrors.Length, "LowerRate should have two validation messages.");
            CollectionAssert.Contains(lowerRateErrors, "must be >= 0");
            CollectionAssert.Contains(lowerRateErrors, "must be less than UpperRate");

            Assert.AreEqual(1, upperRateErrors.Length, "UpperRate should have one validation message.");
            Assert.AreEqual("must be <= 100", upperRateErrors[0]);
        }

        [TestMethod]
        public void Build_WithValidationException_DoesNotReturnNullDetails()
        {
            // Arrange
            var failures = new[]
            {
                new ValidationFailure("SomeProperty", "some error")
            };

            var validationException = new ValidationException(failures);

            // Act
            var response = _builder.Build(validationException);

            // Assert
            Assert.IsNotNull(response, "Response should not be null.");
            Assert.IsNotNull(response.Details, "Details dictionary should be initialized.");
        }
    }
}