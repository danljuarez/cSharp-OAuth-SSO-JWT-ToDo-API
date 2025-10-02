using System.Net;

namespace OauthSSOJwtTodoApiBackend.Tests.xUnit.TestUtils;

// Fake handler for simulating HTTP responses
public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;

    public FakeHttpMessageHandler(HttpResponseMessage singleResponse)
    {
        _responses = new Queue<HttpResponseMessage>();
        _responses.Enqueue(singleResponse);
    }

    public FakeHttpMessageHandler(IEnumerable<HttpResponseMessage> responses)
    {
        _responses = new Queue<HttpResponseMessage>(responses);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(
            _responses.Count > 0
                ? _responses.Dequeue()
                : new HttpResponseMessage(HttpStatusCode.NotFound)
        );
    }
}
