using GitRepoTask.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using Moq;
using Newtonsoft.Json;

namespace GitRepoTask.Tests.Services
{
    public class SampleResponse
    {
        public string Message { get; set; }
    }

    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendAsyncFunc { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsyncFunc(request, cancellationToken);
        }
    }

    [TestClass]
    public class RestServiceTests
    {
        private Mock<ILoggingService> _mockLoggingService;

        [TestInitialize]
        public void Setup()
        {
            _mockLoggingService = new Mock<ILoggingService>();
        }

        [TestMethod]
        public async Task GetAsync_ReturnsDeserializedObject_WhenResponseIsSuccess()
        {
            var expectedResponse = new SampleResponse { Message = "Hello, world!" };
            var jsonResponse = JsonConvert.SerializeObject(expectedResponse);

            var fakeHandler = new FakeHttpMessageHandler
            {
                SendAsyncFunc = (request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(jsonResponse)
                    };
                    return Task.FromResult(response);
                }
            };

            var httpClient = new HttpClient(fakeHandler);
            var restService = new RestService(httpClient, _mockLoggingService.Object);
            var apiUrl = new Uri("http://dummyapi.com");

            var result = await restService.GetAsync<SampleResponse>(apiUrl);

            Assert.IsNotNull(result, "Expected a non-null deserialized object.");
            Assert.AreEqual(expectedResponse.Message, result.Message, "Expected the message to match the deserialized value.");
        }

        [TestMethod]
        public async Task GetAsync_ReturnsDefault_WhenResponseIsNotSuccess()
        {
            var fakeHandler = new FakeHttpMessageHandler
            {
                SendAsyncFunc = (request, cancellationToken) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
                    return Task.FromResult(response);
                }
            };

            var httpClient = new HttpClient(fakeHandler);
            var restService = new RestService(httpClient, _mockLoggingService.Object);
            var apiUrl = new Uri("http://dummyapi.com");

            var result = await restService.GetAsync<SampleResponse>(apiUrl);

            Assert.IsNull(result, "Expected default (null for reference types) when response status is not successful.");
        }

        [TestMethod]
        public async Task GetAsync_LogsErrorAndReturnsDefault_WhenExceptionIsThrown()
        {
            var fakeHandler = new FakeHttpMessageHandler
            {
                SendAsyncFunc = (request, cancellationToken) =>
                {
                    throw new HttpRequestException("mock exception");
                }
            };

            var httpClient = new HttpClient(fakeHandler);
            var restService = new RestService(httpClient, _mockLoggingService.Object);
            var apiUrl = new Uri("http://dummyapi.com");

            var result = await restService.GetAsync<SampleResponse>(apiUrl);

            Assert.IsNull(result, "Expected default (null for reference types) when an exception is thrown.");
            _mockLoggingService.Verify(
                x => x.LogError(It.Is<string>(s => s.Contains(apiUrl.ToString())), It.IsAny<Exception>()),
                Times.Once,
                "Expected LogError to be called once when an exception is thrown.");
        }
    }
}