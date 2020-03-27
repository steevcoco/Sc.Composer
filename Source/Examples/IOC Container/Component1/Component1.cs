using System;
using ComponentInterfaces;


namespace Component1
{
    internal class Component1
        : IComponent1
    {
        public Component1()
            => Id = Guid.NewGuid();


        public Guid Id { get; }
    }
}
