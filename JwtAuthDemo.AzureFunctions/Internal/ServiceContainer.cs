using System;

namespace JwtAuthDemo.AzureFunctions.Internal
{
    internal class ServiceContainer<T>
    {
        internal T Service { get; }

        internal ServiceContainer(T service)
        {
            Service = service ?? throw new ArgumentNullException(nameof(service));
        }
    }
}