using System;
using System.Collections.Generic;
using System.Linq;
using Sc.Util.Collections;


namespace Sc.Util.System
{
	/// <summary>
	/// Static utilities for working with Exceptions.
	/// </summary>
	public static class ExceptionHelper
	{
		/// <summary>
		/// The <see cref="AggregateException"/> is re-created, with the optional <c>message</c>. Note that the
		/// incoming <see cref="AggregateException"/> may be null: if so, a new instance is created. The <c>error</c>
		/// argument is inserted into the <see cref="AggregateException.InnerExceptions"/> list at the beginning. If
		/// the <c>message</c> is not null, it is set as the <see cref="Exception.Message"/>; and if null, the
		/// <c>error</c>'s Message is used.
		/// </summary>
		/// <param name="aggregateException">Notice: can be null.</param>
		/// <param name="error">Not null</param>
		/// <param name="message">If null or whitespace, will be set to the <c>error</c>'s Message.</param>
		/// <returns>Not null.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static AggregateException AggregateError(
				this AggregateException aggregateException,
				Exception error,
				string message = null)
			=> aggregateException.AggregateError(
					(newMessage, innerExceptions) => new AggregateException(newMessage, innerExceptions),
					error,
					message);

		/// <summary>
		/// The <see cref="AggregateException"/> is re-created, with the optional <c>message</c>. Note that the
		/// incoming <see cref="AggregateException"/> may be null: if so, a new instance is created. The <c>error</c>
		/// argument is inserted into the <see cref="AggregateException.InnerExceptions"/> list at the beginning. If
		/// the <c>message</c> is not null, it is set as the <see cref="Exception.Message"/>; and if null, the
		/// <c>error</c>'s Message is used.
		/// </summary>
		/// <typeparam name="TAggregateException">Your <see cref="AggregateException"/> type.</typeparam>
		/// <param name="aggregateException">Notice: can be null.</param>
		/// <param name="constructor">Not null. This must construct your new <see cref="TAggregateException"/>
		/// instance, with the final Message and List of InnerExceptions.</param>
		/// <param name="error">Not null</param>
		/// <param name="message">If null or whitespace, will be set to the <c>error</c>'s Message.</param>
		/// <returns>The result from your constructor..</returns>
		/// <exception cref="ArgumentNullException"></exception>
		public static TAggregateException AggregateError<TAggregateException>(
				this TAggregateException aggregateException,
				Func<string, IEnumerable<Exception>, TAggregateException> constructor,
				Exception error,
				string message = null)
				where TAggregateException : AggregateException
		{
			if (constructor == null)
				throw new ArgumentNullException(nameof(constructor));
			if (error == null)
				throw new ArgumentNullException(nameof(error));
			if (string.IsNullOrWhiteSpace(message))
				message = error.Message;
			return constructor(
					message,
					error.AsSingle()
							.Concat(
									aggregateException?.InnerExceptions
									?? EnumerableHelper.EmptyEnumerable<Exception>()));
		}


		/// <summary>
		/// Checks if any item is null, and will throw <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <typeparam name="T1">ValueTuple item type.</typeparam>
		/// <param name="valueTuple">This ValueTuple.</param>
		public static void ThrowIfAnyItemNull<T1>(this ValueTuple<T1> valueTuple)
		{
			if (valueTuple.Item1 == null)
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1>.Item1)}");
		}

		/// <summary>
		/// Checks if any item is null, and will throw <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <typeparam name="T1">ValueTuple item type.</typeparam>
		/// <typeparam name="T2">ValueTuple item type.</typeparam>
		/// <param name="valueTuple">This ValueTuple.</param>
		/// <param name="skipItemNumbers">An optional list of item numbers to skip checking:
		/// item numbers start at 1.</param>
		public static void ThrowIfAnyItemNull<T1, T2>(
				this ValueTuple<T1, T2> valueTuple,
				params int[] skipItemNumbers)
		{
			if ((valueTuple.Item1 == null)
					&& !skipItemNumbers.Contains(1)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1>.Item1)}");
			}
			if ((valueTuple.Item2 == null)
					&& !skipItemNumbers.Contains(2)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2>.Item2)}");
			}
		}

		/// <summary>
		/// Checks if any item is null, and will throw <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <typeparam name="T1">ValueTuple item type.</typeparam>
		/// <typeparam name="T2">ValueTuple item type.</typeparam>
		/// <typeparam name="T3">ValueTuple item type.</typeparam>
		/// <param name="valueTuple">This ValueTuple.</param>
		/// <param name="skipItemNumbers">An optional list of item numbers to skip checking:
		/// item numbers start at 1.</param>
		public static void ThrowIfAnyItemNull<T1, T2, T3>(
				this ValueTuple<T1, T2, T3> valueTuple,
				params int[] skipItemNumbers)
		{
			if ((valueTuple.Item1 == null)
					&& !skipItemNumbers.Contains(1)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1>.Item1)}");
			}
			if ((valueTuple.Item2 == null)
					&& !skipItemNumbers.Contains(2)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2>.Item2)}");
			}
			if ((valueTuple.Item3 == null)
					&& !skipItemNumbers.Contains(3)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2, T3>.Item3)}");
			}
		}

		/// <summary>
		/// Checks if any item is null, and will throw <see cref="ArgumentNullException"/>.
		/// </summary>
		/// <typeparam name="T1">ValueTuple item type.</typeparam>
		/// <typeparam name="T2">ValueTuple item type.</typeparam>
		/// <typeparam name="T3">ValueTuple item type.</typeparam>
		/// <typeparam name="T4">ValueTuple item type.</typeparam>
		/// <param name="valueTuple">This ValueTuple.</param>
		/// <param name="skipItemNumbers">An optional list of item numbers to skip checking:
		/// item numbers start at 1.</param>
		public static void ThrowIfAnyItemNull<T1, T2, T3, T4>(
				this ValueTuple<T1, T2, T3, T4> valueTuple,
				params int[] skipItemNumbers)
		{
			if ((valueTuple.Item1 == null)
					&& !skipItemNumbers.Contains(1)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1>.Item1)}");
			}
			if ((valueTuple.Item2 == null)
					&& !skipItemNumbers.Contains(2)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2>.Item2)}");
			}
			if ((valueTuple.Item3 == null)
					&& !skipItemNumbers.Contains(3)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2, T3>.Item3)}");
			}
			if ((valueTuple.Item4 == null)
					&& !skipItemNumbers.Contains(4)) {
				throw new ArgumentNullException($"{nameof(ValueTuple)}.{nameof(ValueTuple<T1, T2, T3, T4>.Item4)}");
			}
		}
	}
}
