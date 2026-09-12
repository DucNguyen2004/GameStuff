using Code.Services.AssetProvider;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Code.Services.Factories
{
    public abstract class Factory
    {
        private readonly IObjectResolver _objectResolver;
        private readonly IAssetProvider _assetProvider;

        protected Factory(IObjectResolver objectResolver, IAssetProvider assetProvider)
        {
            _objectResolver = objectResolver;
            _assetProvider = assetProvider;
        }

        protected async UniTask<GameObject> Instantiate(string address)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_objectResolver.Instantiate(prefab));
        }

        protected async UniTask<GameObject> Instantiate(string address, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_objectResolver.Instantiate(prefab, parent));
        }

        protected async UniTask<GameObject> Instantiate(string address, Transform parent, bool isCanvas)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            GameObject instance = _objectResolver.Instantiate(prefab, parent);
            return isCanvas ? instance : MoveToCurrentScene(instance);
        }

        protected async UniTask<GameObject> Instantiate(string address, Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(address);
            return MoveToCurrentScene(_objectResolver.Instantiate(prefab, position, rotation, parent));
        }

        protected async UniTask<GameObject> Instantiate(AssetReference assetReference, Transform parent)
        {
            GameObject prefab = await _assetProvider.Load<GameObject>(assetReference);
            return _objectResolver.Instantiate(prefab, parent);
        }

        protected GameObject Instantiate(GameObject prefab)
        {
            return MoveToCurrentScene(_objectResolver.Instantiate(prefab));
        }

        protected GameObject Instantiate(GameObject prefab, Transform parent) =>
            _objectResolver.Instantiate(prefab, parent);

        private GameObject MoveToCurrentScene(GameObject gameObject)
        {
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            return gameObject;
        }
    }
}
