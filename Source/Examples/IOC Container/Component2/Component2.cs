using System;
using ComponentInterfaces;


namespace Component2
{
    internal class Component2
        : IComponent2
    {
        public Component2()
            => Id = Guid.NewGuid();


        public Guid Id { get; }
    }
}
