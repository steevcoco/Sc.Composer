using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Sc.Util.Events
{
	/// <summary>
	/// This is an <see cref="EventArgs"/> subclass that defines a list of
	/// <see cref="Task"/>. When raised,
	/// the producer must await any Tasks. Any handler can add a
	/// Task here, and when the raised event returns to the producer,
	/// the producer is expected to await all Tasks before completing
	/// the raised event. Notice that consumers must add Tasks to
	/// the event synchronously on the Thread invoking the event:
	/// the producer will fetch all Tasks
	/// immediately when the event returns.
	/// </summary>
	public class AwaitedEventArgs
			: EventArgs
	{
		private readonly List<Task> awaitedsList = new List<Task>(1);


		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="getTasks">Not null. This returns a delegate that will
		/// fetch all added Tasks when invoked. Note that when invoked,
		/// the Tasks will be returned and then cleared from this event.</param>
		public AwaitedEventArgs(out Func<IEnumerable<Task>> getTasks)
			=> getTasks = this.getTasks;


		private IEnumerable<Task> getTasks()
		{
			lock (awaitedsList) {
				Task[] result = awaitedsList.ToArray();
				awaitedsList.Clear();
				return result;
			}
		}


		/// <summary>
		/// For handlers: adds the <see cref="Task"/> to be
		/// awaited by the event producer.
		/// </summary>
		/// <param name="task">Not null.</param>
		/// <exception cref="ArgumentNullException"></exception>
		public void AddTask(Task task)
		{
			if (task == null)
				throw new ArgumentNullException(nameof(task));
			lock (awaitedsList) {
				if (!awaitedsList.Contains(task))
					awaitedsList.Add(task);
			}
		}
	}
}
