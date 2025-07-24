using GTreasury.Api.Functions.Extensions;
using GTreasury.Api.Functions.Validators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GTreasury.Api.Tests.Extensions
{
    [TestClass]
    public class ValidatorRegistryExtensionTests
    {
        [DataTestMethod]
        [DataRow(typeof(CashFlowValidator))]
        [DataRow(typeof(NpvInputValidator))]
        public void AddValidators_ShouldRegisterCashFlowValidatorAsTransient(Type validatorType)
        {
            // Arrange
            var serviceProvider = BuildServiceProviderWithValidators();

            // Act
            var instance1 = serviceProvider.GetService(validatorType);
            var instance2 = serviceProvider.GetService(validatorType);

            // Assert
            Assert.IsNotNull(instance1, $"{validatorType.Name} should be registered.");
            Assert.IsNotNull(instance2, $"{validatorType.Name} should be registered.");
            Assert.AreNotSame(instance1, instance2, $"{validatorType.Name} should be transient (new instance each time).");
        }

        [TestMethod]
        public void AddValidators_ShouldRegisterAllRequiredValidators()
        {
            // Arrange
            var serviceProvider = BuildServiceProviderWithValidators();

            // Assert
            Assert.IsNotNull(serviceProvider.GetService<CashFlowValidator>(), "CashFlowValidator not registered.");
            Assert.IsNotNull(serviceProvider.GetService<NpvInputValidator>(), "NpvInputValidator not registered.");
        }

        private ServiceProvider BuildServiceProviderWithValidators()
        {
            var services = new ServiceCollection();
            services.AddValidators();
            return services.BuildServiceProvider();
        }
    }
}