using System.ComponentModel.DataAnnotations;

namespace backend.Options
{
    public sealed class YahooOptions
    {
        public const string SectionName = "Yahoo";

        [Required, Url]

        public string BaseUrl { get; set; } = "https://query1.finance.yahoo.com/";
        [Required] // added user agent to avoid 403 forbidden errors from Yahoo Finance API
        public string userAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3";
        [Required]
        public string Interval { get; set; } = "15m";
        [Required]
        public string Range { get; set; } = "1mo";
        [Required]
        public int TimeoutSeconds { get; set; } = 30;
    }
}
