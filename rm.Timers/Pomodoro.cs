using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace rm.Timers
{
	/// <summary>
	/// Pomodoro timer.
	/// </summary>
	public class Pomodoro : IDisposable
	{
		private Queue<int> tasks;
		private Stopwatch stopwatch;
		private Timer timer;
		private uint roundTimeInSeconds;
		private uint breakTimeInSeconds;
		private uint longbreakTimeInSeconds;
		private ushort numberOfRounds;

		/// <summary>
		/// Index for current round.
		/// </summary>
		private int roundi;

		/// <summary>
		/// Bool to denote if round or break is in progress.
		/// </summary>
		private bool isInRound = false;

		public Action<string> Notifier;

		/// <summary>
		/// Creates a new pomodoro timer.
		/// </summary>
		public Pomodoro(
			uint roundTimeInSeconds,
			uint breakTimeInSeconds,
			uint longbreakTimeInSeconds,
			ushort numberOfRounds)
		{
			tasks = new Queue<int>();
			timer = new Timer();
			stopwatch = new Stopwatch();

			this.roundTimeInSeconds = roundTimeInSeconds;
			this.breakTimeInSeconds = breakTimeInSeconds;
			this.longbreakTimeInSeconds = longbreakTimeInSeconds;
			this.numberOfRounds = numberOfRounds;
		}

		/// <summary>
		/// Starts the timer.
		/// </summary>
		public void Start()
		{
			if (!tasks.Any())
			{
				throw new InvalidOperationException("No tasks added.");
			}
			StopTicking();
			Notify("starting...");
			SubscribeHandler();
			ResetInner();
			StartRound();
		}

		/// <summary>
		/// Stops the timer.
		/// </summary>
		public void Stop()
		{
			StopTicking();
			Notify("stopping...");
			DisposeInner();
		}

		/// <summary>
		/// Resets the timer.
		/// </summary>
		public void Reset()
		{
			StopTicking();
			Notify("reseting...");
			ResetInner();
			StartRound();
		}

		/// <summary>
		/// Returns the remaining time in current interval (round or break).
		/// </summary>
		public TimeSpan GetRemainingTime()
		{
			var remaining = GetIntervalInTimeSpan().Subtract(stopwatch.Elapsed);
			Notify($"{remaining.Minutes} mins, {remaining.Seconds} secs left...");
			return remaining;
		}

		public void AddTask(int task)
		{
			Notify($"adding task {task}...");
			tasks.Enqueue(task);
		}

		public void FinishTask()
		{
			if (!tasks.Any())
			{
				throw new InvalidOperationException("No more tasks left.");
			}
			var task = tasks.Dequeue();
			Notify($"finishing task {task}..., {tasks.Count} still remaining...");
		}

		/// <summary>
		/// Wrapper method to start round or break.
		/// </summary>
		private void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			if (!isInRound)
			{
				StartRound();
			}
			else
			{
				StartBreak();
			}
		}

		private void StartRound()
		{
			StopTicking();
			NextRound();
			Notify($"starting round {roundi}...");
			isInRound = !isInRound;
			timer.Interval = GetIntervalInMilliseconds();
			StartTicking();
		}

		private void StartBreak()
		{
			StopTicking();
			Notify($"starting{(roundi + 1 == numberOfRounds ? " long" : "")} break {roundi}...");
			isInRound = !isInRound;
			timer.Interval = TimeSpan.FromSeconds(GetIntervalInSeconds()).TotalMilliseconds;
			StartTicking();
		}

		private void SubscribeHandler()
		{
			timer.Elapsed -= OnTimedEvent;
			timer.Elapsed += OnTimedEvent;
		}

		/// <remarks>
		/// We want to increment before starting a round to get correct round or break
		/// interval so <see cref="roundi"/> is -1.
		/// </remarks>
		private void ResetInner()
		{
			roundi = -1;
			isInRound = false;
		}

		/// <summary>
		/// Increments <see cref="roundi"/> and wraps around <see cref="numberOfRounds"/>.
		/// </summary>
		private void NextRound()
		{
			roundi = (roundi + 1) % numberOfRounds;
		}

		/// <summary>
		/// Returns the current interval (round's or break's interval) in seconds
		/// depending on what is in process.
		/// </summary>
		private uint GetIntervalInSeconds()
		{
			uint seconds = 0;
			if (isInRound)
			{
				seconds = roundTimeInSeconds;
			}
			else
			{
				if (roundi + 1 < numberOfRounds)
				{
					seconds = breakTimeInSeconds;
				}
				else
				{
					seconds = longbreakTimeInSeconds;
				}
			}
			return seconds;
		}

		/// <summary>
		/// Returns the current interval (round's or break's interval) in TimeSpan
		/// depending on what is in process.
		/// </summary>
		private TimeSpan GetIntervalInTimeSpan()
		{
			return TimeSpan.FromSeconds(GetIntervalInSeconds());
		}

		private double GetIntervalInMilliseconds()
		{
			return GetIntervalInTimeSpan().TotalMilliseconds;
		}

		/// <summary>
		/// Starts timer and stopwatch.
		/// </summary>
		private void StartTicking()
		{
			timer.Start();
			stopwatch.Reset();
			stopwatch.Start();
			timer.Enabled = true;
		}

		/// <summary>
		/// Stops timer and stopwatch.
		/// </summary>
		private void StopTicking()
		{
			timer.Enabled = false;
			timer.Stop();
			stopwatch.Stop();
		}

		private void Notify(string s)
		{
			if (Notifier != null)
			{
				Notifier(s);
			}
		}

		#region dispose

		// see https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose

		bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing)
			{
				// Free any other managed objects here.
				Stop();
			}
			// Free any unmanaged objects here.
			disposed = true;
		}

		private void DisposeInner()
		{
			timer?.Dispose();
		}

		#endregion
	}
}
