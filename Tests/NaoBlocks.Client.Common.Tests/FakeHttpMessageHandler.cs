using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NaoBlocks.Client.Common.Tests
{
    public class FakeHttpMessageHandler
        : HttpMessageHandler
    {
        public List<HttpRequestMessage> IncomingMessages { get; } = new List<HttpRequestMessage>();

        public Queue<HttpResponseMessage> OutgoingMessages { get; set; } = new Queue<HttpResponseMessage>();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.IncomingMessages.Add(request);
            return Task.FromResult(this.OutgoingMessages.Dequeue());
        }

        public void AddOutgoing(HttpStatusCode statusCode, HttpContent content)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = content
            };
            this.OutgoingMessages.Enqueue(response);
        }

        public void AddOutgoing(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content)
            };
            this.OutgoingMessages.Enqueue(response);
        }
    }
}
