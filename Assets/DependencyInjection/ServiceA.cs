using UnityEngine;

namespace DependencyInjection
{
    public interface IServiceA
    {
        void Initialize(string message = null);
    }

    public class ServiceA : IServiceA
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceA.Initialize({message})");
        }
    }

}