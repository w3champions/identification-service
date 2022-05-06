using System;
using System.Net;

namespace W3ChampionsIdentificationService.Middleware
{
    public class HttpException : Exception
    {
        private readonly int httpStatusCode;

        public HttpException(int _httpStatusCode)
        {
            httpStatusCode = _httpStatusCode;
        }

        public HttpException(HttpStatusCode _httpStatusCode)
        {
            httpStatusCode = (int)_httpStatusCode;
        }

        public HttpException(int _httpStatusCode, string message) : base(message)
        {
            httpStatusCode = _httpStatusCode;
        }

        public HttpException(HttpStatusCode _httpStatusCode, string message) : base(message)
        {
            httpStatusCode = (int)_httpStatusCode;
        }

        public HttpException(int _httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            httpStatusCode = _httpStatusCode;
        }

        public HttpException(HttpStatusCode _httpStatusCode, string message, Exception inner) : base(message, inner)
        {
            httpStatusCode = (int)_httpStatusCode;
        }

        public int StatusCode { get { return httpStatusCode; } }
    }
}