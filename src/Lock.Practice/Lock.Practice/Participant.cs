
namespace Lock.Practice
{
	internal class Participant
	{
		public readonly int Bids;
		public readonly double BidAmount;

		public Participant(int bids, double bidAmount)
		{
			Bids = bids;
			BidAmount = bidAmount;
		}
	}
}
