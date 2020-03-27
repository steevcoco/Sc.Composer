namespace Sc.Util.System
{
	/// <summary>
	/// Defines a <see langword="delegate"/> that implements a boolean-returning
	/// predicate, which also receives a current <typeparamref name="T"/>
	/// value, and if the predicate returns true, it can provide a
	/// new value.
	/// </summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="currentValue">CAN be null: the current value.</param>
	/// <param name="newValue">CAN be null even if the predicate returns
	/// true; but if the predicate does return true, this is taken as
	/// the new value to replace the current value.</param>
	/// <returns>True if the new value should replace the current value.</returns>
	public delegate bool ValuePredicate<T>(T currentValue, out T newValue);

	/// <summary>
	/// Defines a <see langword="delegate"/> that implements a boolean-returning
	/// predicate, which also receives a current <typeparamref name="TIn"/>
	/// value, and if the predicate returns true, it can provide a
	/// new value.
	/// </summary>
	/// <typeparam name="TIn">The input predicate value type.</typeparam>
	/// <typeparam name="TOut">The output value type.</typeparam>
	/// <param name="currentValue">CAN be null: the current value.</param>
	/// <param name="newValue">CAN be null even if the predicate returns
	/// true; but if the predicate does return true, this is taken as
	/// the new value to replace the current value.</param>
	/// <returns>True if the new value should replace the current value.</returns>
	public delegate bool ValuePredicate<in TIn, TOut>(TIn currentValue, out TOut newValue);
}
