using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using TMSTeacher.Controllers;
using TMS_SharedLibrary.Models;
using Xunit;

namespace TMS_TEST.TMSTeacher.ControllerTests
{
    public class ToyImagesControllerTests
    {
        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

            public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
            {
                _handler = handler;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_handler(request));
            }
        }

        private sealed class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly HttpClient _client;

            public StubHttpClientFactory(HttpClient client)
            {
                _client = client;
            }

            public HttpClient CreateClient(string name) => _client;
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenFileNameMissing()
        {
            var handler = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new StubHttpClientFactory(client);
            var logger = new Mock<ILogger<ToyImagesController>>();  

            var controller = new ToyImagesController(factory, logger.Object); 

            var result = await controller.Get("");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenApiReturnsNonSuccess()
        {
            var handler = new StubHttpMessageHandler(req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("Uploads/toy-image/", req.RequestUri!.ToString());
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new StubHttpClientFactory(client);
            var logger = new Mock<ILogger<ToyImagesController>>(); 

            var controller = new ToyImagesController(factory, logger.Object); 

            var result = await controller.Get("missing.png");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Get_ReturnsFile_WhenApiReturnsSuccess()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var handler = new StubHttpMessageHandler(req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                var resp = new HttpResponseMessage(HttpStatusCode.OK);
                resp.Content = new ByteArrayContent(bytes);
                resp.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                return resp;
            });
            var client = new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") };
            var factory = new StubHttpClientFactory(client);
            var logger = new Mock<ILogger<ToyImagesController>>();  

            var controller = new ToyImagesController(factory, logger.Object);  

            var result = await controller.Get("toy.png");

            var file = Assert.IsType<FileContentResult>(result);
            Assert.Equal("image/png", file.ContentType);
            Assert.Equal(bytes, file.FileContents);
        }
    }
}