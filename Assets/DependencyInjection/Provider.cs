using UnityEngine;

namespace DependencyInjection
{
    public class Provider : MonoBehaviour, IDependencyProvider
    {
        [Provide]
        public IServiceA ProvideServiceA()
        {
            return new ServiceA();
        }

        [Provide]
        public IServiceB ProvideServiceB()
        {
            return new ServiceB();
        }

        [Provide]
        public FactoryA ProvideFactoryA()
        {
            return new FactoryA();
        }

        // Other provides...
    }

    public class FactoryA
    {
        ServiceA cachedServiceA;

        public ServiceA CreateServiceA()
        {
            if (cachedServiceA == null)
            {
                cachedServiceA = new ServiceA();
            }
            return cachedServiceA;
        }
    }

}