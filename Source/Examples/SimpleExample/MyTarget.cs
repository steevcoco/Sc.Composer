using System;
using System.Collections.Generic;
using Sc.Util.Collections;


namespace SimpleExample
{
	public class MyTarget
	{
		private readonly List<MyPart> parts = new List<MyPart>();
		private bool isBootstrapped;


		public void AddPart(MyPart part)
			=> parts.Add(part ?? throw new ArgumentNullException(nameof(part)));

		public void Bootstrap()
			=> isBootstrapped = true;

		public bool IsBootstrapped
			=> isBootstrapped;


		public override string ToString()
			=> $"{nameof(MyTarget)}[{nameof(IsBootstrapped)}: {IsBootstrapped}, {parts.ToStringCollection()}]";
	}
}
