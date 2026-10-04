using System.Net;

namespace backend.Exceptions
{
    public sealed class SymbolNotFoundException : Exception
    {
        public SymbolNotFoundException(string symbol)
            : base($"Stock symbol '{symbol}' was not found.")
        {
            Symbol = symbol;
        }

        public string Symbol { get; }
        public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
    }

    public sealed class UpstreamServiceException : Exception
    {
        public UpstreamServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public HttpStatusCode StatusCode => HttpStatusCode.BadGateway;
    }
}
