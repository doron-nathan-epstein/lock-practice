using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BenchmarkDotNet.Attributes;

namespace Lock.Practice
{
	[MemoryDiagnoser]
	public class Auction
	{
		private readonly IEnumerable<Participant> _participants;
		private readonly AuctionItem _item;

		private int _lastThreadId = 0;

		private static readonly object _lockLockObject = new object();
		private int _interlockLock = 0;
		private readonly object _monitorLockObject = new object();


		public Auction(int participantCount = 5)
		{
			var r = new Random();
			_participants = Enumerable.Range(0, participantCount).Select(p => new Participant(r.Next(10), 100));

			_item = new AuctionItem
			{
				Name = "TV",
				StartingPrice = 100.00D,
				Price = 100.00D
			};
		}

		//[Benchmark]
		public void RunAuctionWithLock()
		{
			foreach (var participant in _participants)
			{
				var t = new Thread(BidWithLock);
				t.Start(participant);
			}
		}

		[Benchmark]
		public void RunAuctionWithInterlock()
		{
			foreach (var participant in _participants)
			{
				var t = new Thread(BidWithInterlock);
				t.Start(participant);
			}
		}

		//[Benchmark]
		public void RunAuctionWithMonitor()
		{
			foreach (var participant in _participants)
			{
				var t = new Thread(BidWithMonitor);
				t.Start(participant);
			}
		}

		private void BidWithLock(object p)
		{
			var participant = (Participant) p;

			for (var i = 0; i < participant.Bids; i++)
			{
				while (Thread.CurrentThread.ManagedThreadId != _lastThreadId)
				{
					lock (_lockLockObject)
					{
						_item.Price += participant.BidAmount;
						_item.LastBidder = Thread.CurrentThread.ManagedThreadId;
						_lastThreadId = Thread.CurrentThread.ManagedThreadId;

						Console.WriteLine(
							$"{_item.Name} last bid by {Thread.CurrentThread.ManagedThreadId}: {_item.Price} (+{participant.BidAmount})");

						break;
					}
				}
			}
		}

		private void BidWithInterlock(object p)
		{
			var participant = (Participant)p;

			for (var i = 0; i < participant.Bids; i++)
			{
				while (Thread.CurrentThread.ManagedThreadId != _lastThreadId)
				{
					if (Interlocked.Exchange(ref _interlockLock, 1) == 0)
					{
						try
						{
							_item.Price += participant.BidAmount;
							_item.LastBidder = Thread.CurrentThread.ManagedThreadId;
							_lastThreadId = Thread.CurrentThread.ManagedThreadId;

							Console.WriteLine($"{_item.Name} last bid by {Thread.CurrentThread.ManagedThreadId}: {_item.Price} (+{participant.BidAmount})");
						}
						finally
						{
							Interlocked.Exchange(ref _interlockLock, 0);
						}
						break;
					}
				}
			}
		}

		private void BidWithMonitor(object p)
		{
			var participant = (Participant)p;

			for (var i = 0; i < participant.Bids; i++)
			{
				while (Thread.CurrentThread.ManagedThreadId != _lastThreadId)
				{
					Monitor.Enter(_monitorLockObject);
					try
					{
						_item.Price += participant.BidAmount;
						_item.LastBidder = Thread.CurrentThread.ManagedThreadId;
						_lastThreadId = Thread.CurrentThread.ManagedThreadId;

						Console.WriteLine($"{_item.Name} last bid by {Thread.CurrentThread.ManagedThreadId}: {_item.Price} (+{participant.BidAmount})");
					}
					finally
					{
						Monitor.Exit(_monitorLockObject);
					}

					break;
				}
			}
		}
	}
}