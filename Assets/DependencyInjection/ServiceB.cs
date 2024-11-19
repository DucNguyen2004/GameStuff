using UnityEngine;

namespace DependencyInjection
{
    public interface IServiceB
    {
        void Initialize(string message = null);
    }
    public class ServiceB : IServiceB
    {
        public void Initialize(string message = null)
        {
            Debug.Log($"ServiceB.Initialize({message})");
        }
    }

}