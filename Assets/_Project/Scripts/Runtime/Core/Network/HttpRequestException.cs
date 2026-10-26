using System;

namespace TestTask.Core.Network
{
    /// <summary>Ошибка сети или HTTP-ответ с кодом ошибки.</summary>
    public sealed class HttpRequestException : Exception
    {
        public HttpRequestException(string url, long statusCode, string error)
            : base($"HTTP request failed ({statusCode}): {error}. Url: {url}")
        {
            Url = url;
            StatusCode = statusCode;
        }

        public string Url { get; }
        public long StatusCode { get; }
    }
}
