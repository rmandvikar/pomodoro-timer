using System;
using System.Linq;
using System.Threading.Tasks;
using rm.Timers;

namespace rm.TimersTest
{
	class Program
	{
		static void Main(string[] args)
		{
			Run();
		}

		private static void Run()
		{
			using (var pomodoro = new Pomodoro(5, 2, 4, 3))
			{
				pomodoro.Notifier = Console.WriteLine;

				var taski = 0;
				for (taski = 0; taski < 10; taski++)
				{
					pomodoro.AddTask(taski);
				}
				//pomodoro.Start();

				var loop = true;
				Console.WriteLine("Type ? or h for (h)elp.");
				while (loop)
				{
					var input = Console.ReadKey().KeyChar;
					switch (char.ToLower(input))
					{
						case 's':
							pomodoro.Start();
							break;
						case 't':
							var remaining = pomodoro.GetRemainingTime();
							break;
						case 'a':
							pomodoro.AddTask(taski++);
							break;
						case 'f':
							pomodoro.FinishTask();
							break;
						case 'e':
							pomodoro.Stop();
							loop = false;
							break;
						case 'q':
							pomodoro.Dispose(); // intentional
							loop = false;
							break;
						case 'r':
							pomodoro.Reset();
							break;
						case '?':
						case 'h':
							Console.WriteLine("Type s to (s)tart timer.");
							Console.WriteLine("Type t to get remaining (t)ime in current interval (round or break).");
							Console.WriteLine("Type e to stop/(e)nd timer.");
							Console.WriteLine("Type a to (a)dd task.");
							Console.WriteLine("Type f to (f)inish task.");
							Console.WriteLine("Type q to (q)uit.");
							Console.WriteLine("Type ? or h for (h)elp.");
							break;
					}
				}
			}
		}
	}
}
