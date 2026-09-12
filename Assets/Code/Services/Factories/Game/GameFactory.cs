using Code.Services.AssetProvider;
using VContainer;

namespace Code.Services.Factories.Game
{
    public class GameFactory : Factory, IGameFactory
    {
        public GameFactory(IObjectResolver objectResolver, IAssetProvider assetProvider) : base(objectResolver, assetProvider)
        {
        }
    }
}
