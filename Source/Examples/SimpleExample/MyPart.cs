using System;


namespace SimpleExample
{
	public class MyPart
	{
		public MyPart()
			=> Id = Guid.NewGuid();


		public Guid Id { get; }


		public override string ToString()
			=> $"{nameof(MyPart)}[{Id}]";
	}
}
