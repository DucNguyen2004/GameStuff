using DependencyInjection;
using UnityEngine;

public class ClassB : MonoBehaviour
{
    [Inject] IServiceA serviceA;

    IServiceB serviceB;
    FactoryA factoryA;

    [Inject] // Method injection supports multiple dependencies
    public void Init(FactoryA factoryA, IServiceB serviceB)
    {
        this.factoryA = factoryA;
        this.serviceB = serviceB;
    }

    void Start()
    {
        serviceA.Initialize("ServiceA initialized from ClassB");
        serviceB.Initialize("ServiceB initialized from ClassB");
        factoryA.CreateServiceA().Initialize("ServiceA initialized from FactoryA");
    }
}