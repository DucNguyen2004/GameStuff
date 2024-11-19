using DependencyInjection;
using UnityEngine;

public class ClassA : MonoBehaviour
{
    [Inject] IServiceA serviceA;

    IServiceB serviceB;

    [Inject]
    public void Init(IServiceB service)
    {
        this.serviceB = service;
    }

    //[Inject]
    //public IServiceC Service { get; private set; }
}