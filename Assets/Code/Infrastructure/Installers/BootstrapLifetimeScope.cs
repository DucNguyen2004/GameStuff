using Code.Infrastructure.StateMachine;
using Code.Infrastructure.StateMachine.Game;
using Code.Infrastructure.StateMachine.Game.States;
using Code.Services.AssetPreloader;
using Code.Services.AssetProvider;
using Code.Services.AudioVibrationFX.Music;
using Code.Services.AudioVibrationFX.Sound;
using Code.Services.AudioVibrationFX.StaticData;
using Code.Services.AudioVibrationFX.Vibration;
using Code.Services.Factories.Game;
using Code.Services.Factories.UIFactory;
using Code.Services.Finish;
using Code.Services.Finish.Lose;
using Code.Services.Finish.Win;
using Code.Services.Input;
using Code.Services.Levels;
using Code.Services.LocalProgress;
using Code.Services.PersistenceProgress;
using Code.Services.PreloaderConductor;
using Code.Services.Providers.Widgets;
using Code.Services.Random;
using Code.Services.SaveLoad;
using Code.Services.StaticData;
using Code.Services.Storage;
using Code.Services.Timer;
using Code.Services.Window;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using VContainer;
using VContainer.Unity;
using Application = UnityEngine.Application;

namespace Code.Infrastructure.Installers
{
    public class BootstrapLifetimeScope : LifetimeScope, IInitializable
    {
        [SerializeField] private CoroutineRunner _coroutineRunner;
        [SerializeField] private LoadingCurtain _curtain;

        private RuntimePlatform Platform => Application.platform;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);

            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("Installer");

            // Registered before anything that may pull in the entry point dispatcher, so that the
            // audio warm-up runs before any IInitializable / ITickable is dispatched.
            builder.RegisterBuildCallback(WarmUpAudioServices);

            CreateEventSystem();
            BindMonoServices(builder);
            BindServices(builder);
            BindGameStateMachine(builder);
            MakeInitializable(builder);
        }

        public void Initialize() => BootstrapGame();

        private void BindMonoServices(IContainerBuilder builder)
        {
            builder.RegisterComponentInNewPrefab<ICoroutineRunner, CoroutineRunner>(
                _ => _coroutineRunner, Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab<ILoadingCurtain, LoadingCurtain>(
                _ => _curtain, Lifetime.Singleton);

            BindSceneLoader(builder);
        }

        private void BindServices(IContainerBuilder builder)
        {
            BindStaticDataService(builder);

            BindInterfacesTo<AssetProvider>(builder);
            BindInterfacesTo<AssetPreloaderService>(builder);
            BindInterfacesTo<AssetPreloaderConductor>(builder);

            BindInterfacesTo<UIFactory>(builder);
            BindInterfacesTo<GameFactory>(builder);
            BindInterfacesTo<WindowService>(builder);
            builder.RegisterEntryPoint<InputService>();
            BindInterfacesTo<RandomService>(builder);
            BindInterfacesTo<UnifiedSaveLoadFacade>(builder);
            BindInterfacesTo<WidgetProvider>(builder);
            BindInterfacesTo<LevelService>(builder);
            BindInterfacesTo<StorageService>(builder);
            BindInterfacesTo<TimeService>(builder);

            BindDataServices(builder);
            BindAudioVibrationService(builder);
            BindFinishService(builder);
        }

        private void BindAudioVibrationService(IContainerBuilder builder)
        {
            BindInterfacesTo<SoundService>(builder);
            BindInterfacesTo<MusicService>(builder);
            BindInterfacesTo<VibrationService>(builder);
        }

        private void BindDataServices(IContainerBuilder builder)
        {
            BindInterfacesTo<PersistenceProgressService>(builder);
            BindInterfacesTo<LevelLocalProgressService>(builder);
        }

        private void BindFinishService(IContainerBuilder builder)
        {
            BindInterfacesTo<FinishService>(builder);
            BindInterfacesTo<WinService>(builder);
            BindInterfacesTo<LoseService>(builder);
        }

        private void BindGameStateMachine(IContainerBuilder builder)
        {
            builder.Register<GameStateFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameStateMachine>();

            BindGameStates(builder);
        }

        private void MakeInitializable(IContainerBuilder builder) =>
            builder.RegisterComponent<IInitializable>(this);

        private void BindSceneLoader(IContainerBuilder builder)
        {
            BindInterfacesTo<SceneLoader>(builder);
        }

        private void BindStaticDataService(IContainerBuilder builder)
        {
            IStaticDataService staticDataService = new StaticDataService();
            staticDataService.LoadData();
            builder.RegisterInstance<IStaticDataService>(staticDataService);

            IAudioVibrationStaticDataService audioVibrationStaticDataService = new AudioVibrationStaticDataService();
            audioVibrationStaticDataService.LoadData();
            builder.RegisterInstance<IAudioVibrationStaticDataService>(audioVibrationStaticDataService);
        }

        private void BindGameStates(IContainerBuilder builder)
        {
            builder.Register<BootstrapState>(Lifetime.Singleton);
            builder.Register<LoadProgressState>(Lifetime.Singleton);
            builder.Register<BootstrapAnalyticState>(Lifetime.Singleton);
            builder.Register<PreLoadGameState>(Lifetime.Singleton);
            builder.Register<LoadMenuState>(Lifetime.Singleton);
            builder.Register<LoadLevelState>(Lifetime.Singleton);
            builder.Register<GameLoopState>(Lifetime.Singleton);
        }

        private void CreateEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
                return;

            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
            DontDestroyOnLoad(go);
        }

        private void WarmUpAudioServices(IObjectResolver container)
        {
            ISoundService soundService = container.Resolve<ISoundService>();
            soundService.Cache2DSounds();
            soundService.CreateSoundsPool();

            IMusicService musicService = container.Resolve<IMusicService>();
            musicService.CacheMusic();
            musicService.CreateMusicRoot();
        }

        private void BootstrapGame() => Container.Resolve<IStateMachine<IGameState>>().Enter<BootstrapState>();

        private static RegistrationBuilder BindInterfacesTo<TImplementation>(IContainerBuilder builder) =>
            builder.Register<TImplementation>(Lifetime.Singleton).AsImplementedInterfaces();
    }
}
