namespace backend.Models
{
    //public api contact
    public sealed record DailySummaryDTO(
        DateOnly Date,
        decimal LowAverage,
        decimal HighAverage,
        long TotalVolume
    );
}