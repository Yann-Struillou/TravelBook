namespace Test.TravelBook.Client
{
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;

    public class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler = handler;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }

        public static HttpResponseMessage JsonResponse<T>(HttpStatusCode status, T content)
        {
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(content),
                    Encoding.UTF8,
                    "application/json")
            };
        }
    }

}
