using GTreasury.Api.Functions.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using System.Text.Json;

namespace GTreasury.Api.Tests.Extensions
{
    [TestClass]
    public class RequestDeserializationTests
    {
        public class TestDto
        {
            public string Name { get; set; } = string.Empty;
        }

        [TestMethod]
        public async Task DeserializeBodyAsync_ReturnsDeserializedObject()
        {
            //Arrange
            var dto = new TestDto { Name = "Test" };
            var json = JsonSerializer.Serialize(dto);
            var mockRequest = CreateMockRequest(json);

            //Act
            var result = await mockRequest.DeserializeRequestBodyAsync<TestDto>();

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Test", result.Name);
        }

        [TestMethod]
        public async Task DeserializeBodyAsync_ThrowsJsonException_OnInvalidJson()
        {
            //Arrange
            var mockRequest = CreateMockRequest("not a json");

            //Act & Assert
            _ = await Assert.ThrowsExactlyAsync<JsonException>(mockRequest.DeserializeRequestBodyAsync<TestDto>);
        }

        [TestMethod]
        public async Task DeserializeBodyAsync_ThrowsJsonException_OnEmptyBody()
        {
            //Arrange
            var mockRequest = CreateMockRequest(string.Empty);

            //Act & Assert
            _ = await Assert.ThrowsExactlyAsync<JsonException>(mockRequest.DeserializeRequestBodyAsync<TestDto>);
        }

        private static HttpRequestData CreateMockRequest(string bodyContent)
        {
            var mockFunctionContext = new Mock<FunctionContext>();

            var mockRequest = new Mock<HttpRequestData>(MockBehavior.Strict, mockFunctionContext.Object);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(bodyContent));
            mockRequest.Setup(r => r.Body).Returns(stream);

            return mockRequest.Object;
        }
    }
}