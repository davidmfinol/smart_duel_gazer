namespace Code.Features.SpeedDuel.Models
{
    public class PlayCard
    {
        public int Id { get; }
        public int CopyNumber { get; }

        public PlayCard(int id, int copyNumber)
        {
            Id = id;
            CopyNumber = copyNumber;
        }
    }
}