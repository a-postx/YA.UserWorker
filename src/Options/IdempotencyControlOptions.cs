namespace YA.UserWorker.Options
{
    public class IdempotencyControlOptions
    {
        public bool? IdempotencyFilterEnabled { get; set; }
        public string IdempotencyHeader { get; set; }
    }
}
