using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class JobSerializer
	{
		JobTimer _timer = new JobTimer();
		Queue<IJob> _jobQueue = new Queue<IJob>();
		object _lock = new object();
		bool _flush = false;

		public void Push(Action action) { Push(new Job(action)); }
		public void Push<T1>(Action<T1> action, T1 t1) { Push(new Job<T1>(action, t1)); }
		public void Push<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Push(new Job<T1, T2>(action, t1, t2)); }
		public void Push<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Push(new Job<T1, T2, T3>(action, t1, t2, t3)); }

		public void PushAfter(Action action, int tickAfter) { PushAfter(new Job(action), tickAfter); }
		public void PushAfter<T1>(Action<T1> action, T1 t1, int tickAfter) { PushAfter(new Job<T1>(action, t1), tickAfter); }
		public void PushAfter<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2, int tickAfter) { PushAfter(new Job<T1, T2>(action, t1, t2), tickAfter); }
		public void PushAfter<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3, int tickAfter) { PushAfter(new Job<T1, T2, T3>(action, t1, t2, t3), tickAfter); }

		public void PushAfter(IJob job, int tickAfter = 0)
		{
			_timer.Push(job, tickAfter);
		}

		public void Push(IJob job)
		{
			lock (_lock)
			{
				_jobQueue.Enqueue(job);
			}
		}

		public void Flush()
		{
			_timer.Flush();

			while (true)
			{
				IJob job = Pop();
				if (job == null)
					return;

				job.Execute();
			}
		}

		IJob Pop()
		{
			lock (_lock)
			{
				if (_jobQueue.Count == 0)
				{
					_flush = false;
					return null;
				}
				return _jobQueue.Dequeue();
			}
		}
	}
}
