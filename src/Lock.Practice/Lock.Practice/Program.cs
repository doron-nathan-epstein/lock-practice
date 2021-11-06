using System;
using System.Collections.Generic;
using BenchmarkDotNet.Running;

namespace Lock.Practice
{
	class Program
	{
		static void Main(string[] args)
		{
			var summary = BenchmarkRunner.Run<Auction>();

			Console.ReadLine();
		}
	}
}
