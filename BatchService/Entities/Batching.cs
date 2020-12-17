namespace BatchService.Entities
{
    public class Batching
    {
        public long StudentId { get; set; }

        public bool IsProcessed { get; set; }

        public Student Student { get; set; }
    }
}
