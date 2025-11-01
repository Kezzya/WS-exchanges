using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;


public class ProxyRoundRobinHttpClientHandler : HttpClientHandler
{
    private readonly List<WebProxy> _proxies;
    private int _currentProxyIndex = -1;
    private readonly ConcurrentDictionary<string, HttpClient> _clientPool;
    private readonly object _lock = new object();
    private readonly ILogger _logger;

    public ProxyRoundRobinHttpClientHandler(List<WebProxy> proxies, ILogger logger)
    {
        _logger = logger;
        _proxies = proxies ?? throw new ArgumentNullException(nameof(proxies));
        _clientPool = new ConcurrentDictionary<string, HttpClient>();
        if (_proxies.Count == 0)
        {
            _logger.LogWarning("Proxies list is empty");
            throw new ArgumentException("Proxies list cannot be empty", nameof(proxies));
        }

        foreach (var proxy in _proxies)
        {
            _clientPool[proxy.Address.ToString()] = CreateHttpClient(proxy.Address.ToString());
        }
    }

    private HttpClient CreateHttpClient(string proxy)
    {
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy(proxy),
            UseProxy = true,
        };

        var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);
        return client;
    }
    private static HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Content = request.Content,
            Version = request.Version
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Клонируем запрос, чтобы избежать модификации оригинального
        var clonedRequest = CloneHttpRequestMessage(request);
        HttpClient client;
        lock (_lock)
        {
            _currentProxyIndex = (_currentProxyIndex + 1) % _proxies.Count;
            var proxy = _proxies[_currentProxyIndex];

            client = _clientPool[proxy.Address.ToString()];
            _logger.LogTrace("New request through proxy:{0}", proxy.Address.ToString());
        }
        return client.Send(clonedRequest, cancellationToken);
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Клонируем запрос, чтобы избежать модификации оригинального
        var clonedRequest = CloneHttpRequestMessage(request);
        HttpClient client;
        lock (_lock)
        {
            _currentProxyIndex = (_currentProxyIndex + 1) % _proxies.Count;
            var proxy = _proxies[_currentProxyIndex];

            client = _clientPool[proxy.Address.ToString()];
            _logger.LogTrace("New request through proxy:{0}", proxy.Address.ToString());
        }
        return await client.SendAsync(clonedRequest, cancellationToken);
    }
}