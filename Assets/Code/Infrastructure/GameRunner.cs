using Code.Infrastructure.Installers;
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace Code.Infrastructure
{
    public class GameRunner : MonoBehaviour
    {
        [SerializeField, NotNull] private BootstrapLifetimeScope _bootstrapLifetimeScope;

        private void Awake()
        {
            if (LifetimeScope.Find<BootstrapLifetimeScope>() == null)
                Instantiate(_bootstrapLifetimeScope);

            Destroy(gameObject);
        }
    }
}
