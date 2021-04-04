namespace YA.UserWorker.Options
{
    public class IdempotencyControlOptions
    {
        public bool? IdempotencyFilterEnabled { get; set; }
        public string ClientRequestIdHeader { get; set; }
    }
}
