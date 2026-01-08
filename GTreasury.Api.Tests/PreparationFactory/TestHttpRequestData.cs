using GTreasury.Api.Utilities.Records;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Moq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;

namespace GTreasury.Api.Tests.PreparationFactory
{
    // Minimal concrete HttpRequestData for tests       
    internal sealed class TestHttpRequestData : HttpRequestData
    {
        public TestHttpRequestData(FunctionContext functionContext, Stream body)
            : base(functionContext)
        {
            Body = body;
            Headers = new HttpHeadersCollection();
            _cookies = new TestHttpCookies();
        }

        public override Stream Body { get; }

        public override HttpHeadersCollection Headers { get; }

        // HttpRequestData.Cookies is of type HttpCookies in your SDK
        private readonly TestHttpCookies _cookies;
        public override IReadOnlyCollection<IHttpCookie> Cookies => _cookies.GetAll().AsReadOnly();

        public override Uri Url { get; } = new("http://localhost");

        public override IEnumerable<ClaimsIdentity> Identities => Enumerable.Empty<ClaimsIdentity>();

        public override string Method { get; } = "POST";

        public override HttpResponseData CreateResponse()
            => new TestHttpResponseData(FunctionContext);
    }

    internal sealed class TestHttpResponseData : HttpResponseData
    {
        public TestHttpResponseData(FunctionContext functionContext)
            : base(functionContext)
        {
            _headers = new HttpHeadersCollection();
            Headers = _headers;
            Body = new MemoryStream();
            Cookies = new TestHttpCookies();
        }

        public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        private HttpHeadersCollection _headers;
        public override HttpHeadersCollection Headers
        {
            get => _headers;
            set => _headers = value;
        }

        // HttpResponseData.Cookies is of type HttpCookies in your SDK
        public override HttpCookies Cookies { get; }

        public override Stream Body { get; set; }
    }

    internal static class TestHttpRequestFactory
    {
        // Factory to be used from PostNpvValuesTests
        public static HttpRequestData CreateHttpRequest(NpvInput input)
        {
            var context = new Mock<FunctionContext>().Object;

            var json = JsonSerializer.Serialize(input);
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

            return new TestHttpRequestData(context, stream);
        }
    }

    internal sealed class TestHttpCookies : HttpCookies
    {
        private readonly List<IHttpCookie> _cookies = new();

        public override void Append(string name, string value)
        {
            var cookie = CreateNew();
            // Use constructor or a method to set Name and Value since interface properties are read-only
            if (cookie is TestHttpCookie testCookie)
            {
                testCookie.SetNameAndValue(name, value);
            }
            Append(cookie);
        }

        public override void Append(IHttpCookie cookie)
        {
            _cookies.Add(cookie);
        }

        public override IHttpCookie CreateNew()
        {
            return new TestHttpCookie();
        }

        // Add this method to allow access to the internal list
        public List<IHttpCookie> GetAll()
        {
            return _cookies;
        }
    }

    // Minimal implementation for IHttpCookie
    internal sealed class TestHttpCookie : IHttpCookie
    {
        // Backing fields for interface properties
        private string _name;
        private string _value;

        public string Name => _name;
        public string Value => _value;
        public string? Domain { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public bool? HttpOnly { get; set; }
        public double? MaxAge { get; set; }
        public string? Path { get; set; }
        public SameSite SameSite { get; set; }
        public bool? Secure { get; set; }

        // Helper method to set Name and Value since they are read-only in the interface
        public void SetNameAndValue(string name, string value)
        {
            _name = name;
            _value = value;
        }
    }
}