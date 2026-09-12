# MobileCoreTemplate → VContainer

Port of [SinlessDevil/MobileCoreTemplate](https://github.com/SinlessDevil/MobileCoreTemplate)
(Zenject / Extenject) onto VContainer 1.19.0, Unity 6000.5.3f1.

## What landed in the project

| Path | Contents |
|---|---|
| `Assets/Code/` | All 175 template scripts, converted |
| `Assets/Resources/StaticData/` | ScriptableObject configs loaded at boot |
| `Assets/Resources/Infrastructure/` | `GameRunner`, `CoroutineRunner`, `BootstrapLifetimeScope` prefabs |
| `Assets/Resources/URP/` | URP pipeline + renderer assets |
| `Assets/ResourcesAddresable/` | Menu/Game scenes, HUDs, UI root, window prefabs |
| `Assets/ResourcesStatic/` | UI prefabs, sprites, animations, sounds |
| `Assets/AddressableAssetsData/` | Addressable groups (`Hud`, `UI`, `Game`, `Menu`) |
| `Assets/Scenes/Initial.unity` | Boot scene |
| `Assets/Shaders/`, `Assets/Tests/` | Shaders; EditMode + PlayMode tests |
| `Assets/Plugins/` | DOTween Pro, NiceVibrations, TextMesh Pro essentials, UI Toolkit, CI config |

Everything from the upstream `Assets/` tree is now present except Zenject-specific assets
(`ProjectContext.prefab`, `EmptySceneContext.prefab`, `ZenjectReflectionBakingSettings.asset`).

## Zenject → VContainer mapping

| Zenject | VContainer | Where |
|---|---|---|
| `MonoInstaller` + `InstallBindings()` | `LifetimeScope` + `Configure(IContainerBuilder)` | `Infrastructure/Installers/BootstrapLifetimeScope.cs` |
| `ProjectContext` prefab (`Resources/`) | `BootstrapLifetimeScope` prefab + `DontDestroyOnLoad` in `Awake()` | `Resources/Infrastructure/BootstrapLifetimeScope.prefab` |
| `SceneContext` / `EmptySceneContext` | *(dropped)* — the root scope injects instantiated prefab hierarchies directly | — |
| `Container.BindInterfacesTo<T>().AsSingle()` | `builder.Register<T>(Lifetime.Singleton).AsImplementedInterfaces()` | `BootstrapLifetimeScope.BindInterfacesTo<T>()` |
| `Container.Bind<T>().AsSingle()` | `builder.Register<T>(Lifetime.Singleton)` | game states, `GameStateFactory` |
| `Container.Bind<I>().FromInstance(x).AsSingle()` | `builder.RegisterInstance<I>(x)` | `StaticDataService`, `AudioVibrationStaticDataService` |
| `FromMethod(() => Container.InstantiatePrefabForComponent<I>(p))` | `builder.RegisterComponentInNewPrefab<I, T>(_ => p, Lifetime.Singleton)` | `CoroutineRunner`, `LoadingCurtain` |
| `Zenject.ITickable` | `VContainer.Unity.ITickable` via `builder.RegisterEntryPoint<T>()` | `GameStateMachine`, `InputService` |
| `Zenject.IInitializable` on the installer | `VContainer.Unity.IInitializable` + `builder.RegisterComponent<IInitializable>(this)` | `BootstrapLifetimeScope` |
| `Container.Resolve<T>()` during install | `builder.RegisterBuildCallback(...)` | audio warm-up |
| `DiContainer` | `IObjectResolver` | `StateFactory`, `GameStateFactory` |
| `IInstantiator.InstantiatePrefab(...)` | `IObjectResolver.Instantiate(...)` | `Services/Factories/Factory.cs` |
| `[Zenject.Inject]` method injection | `[VContainer.Inject]` — same shape, no code change | 12 UI/window scripts |
| `IFactory<Type, IExitable>` | *(dropped)* — `IStateFactory` alone | `StateFactory` |

### Ordering note

`EntryPointsBuilder.EnsureDispatcherRegistered` is called by `LifetimeScope.InstallTo` **after**
`Configure`, but `RegisterEntryPoint` also registers it eagerly. The audio warm-up callback is
therefore registered as the **first** statement in `Configure`, so it still runs before any
`IInitializable.Initialize()` / `ITickable.Tick()` — matching Zenject, where it ran inline during
`InstallBindings`.

### Boot flow

`Initial.unity` → `GameRunner` (prefab instance) → if no `BootstrapLifetimeScope` exists,
instantiate it → `LifetimeScope.Awake` builds the container → `IInitializable.Initialize()` →
`IStateMachine<IGameState>.Enter<BootstrapState>()`.

`GameRunner` is also present in `Menu.unity` and `Game.unity`, so those scenes can be entered
directly in the editor; it destroys itself once a scope exists.

## Project settings changed

- `Packages/manifest.json` — added `com.unity.addressables` 2.9.1,
  `com.unity.inputsystem` 1.19.0, `com.unity.nuget.newtonsoft-json` 3.2.2,
  `com.unity.render-pipelines.universal` 17.5.0, `com.cysharp.unitask` (git).
- `ProjectSettings.asset` — `activeInputHandler: 0 → 1` (Input System package).
  **Requires an editor restart.**
- `EditorBuildSettings.asset` — `Assets/Scenes/Initial.unity` as the only build scene;
  Addressables config object wired up.
- `GraphicsSettings.asset` — URP global settings assigned.
- `QualitySettings.asset` — `URP Asset` assigned on all six quality levels.

Assembly definitions (`Game`, `GameEditor`, `Tests.PlayMode`) were rewritten to use **name-based**
references instead of the template's GUID references, since those GUIDs pointed at Zenject and at
packages resolved differently here.

## Fixes needed on top of the straight port

Two things broke that were not Zenject-related, both found by compiling:

1. **`Game.asmdef` needed a second NiceVibrations assembly.** `MMVibrationManager` lives in
   `MoreMountains.NiceVibrations`, but the `HapticTypes` enum used by `VibrationData` lives in
   `MoreMountains.NiceVibrations.Haptics`. Both are referenced now.
2. **`TestsToolWindow.cs` had an ambiguous `TestMode`.** Unity 6.5 ships test-framework 1.7.0,
   which added `UnityEngine.TestTools.TestMode` alongside the existing
   `UnityEditor.TestTools.TestRunner.Api.TestMode`; the template was written against 1.3.9 where
   only one existed. Resolved with a `using TestMode = UnityEditor.TestTools.TestRunner.Api.TestMode;`
   alias.

## Before it runs

- **Addressables content** — `Window → Asset Management → Addressables → Groups`,
  then `Build → New Build → Default Build Script`. For fast iteration set
  *Play Mode Script* to **Use Asset Database**.
- **Restart the editor once** if it was open when `activeInputHandler` changed — Unity only picks
  up the input backend switch on startup.
- TextMesh Pro essentials came in with `Assets/Plugins/TextMesh Pro/`, so `LiberationSans SDF`
  and the TMP shaders resolve without running *Import TMP Essential Resources*. Don't run that
  importer as well, or you will end up with two copies of the same assets.

## Verification

Verified by actually building it: `Unity.exe -batchmode -quit -projectPath D:\GameStuff` on 6000.5.3f1.
Packages resolved and downloaded (URP 17.5.0 + core + universal-config, Addressables 2.9.1,
Input System 1.19.0, UniTask, Newtonsoft 3.2.2), all assets imported, all script assemblies compiled.

**Final state: 0 errors, batchmode exits with return code 0.**

```
Library/ScriptAssemblies/Game.dll
Library/ScriptAssemblies/GameEditor.dll
Library/ScriptAssemblies/Tests.EditMode.dll
Library/ScriptAssemblies/Tests.PlayMode.dll
```

How it got there:

| Pass | Errors | What they were |
|---|---|---|
| 1 — no plugins | 10 | `DG` x7, `MoreMountains` x2, `HapticTypes` x1 — the excluded plugins only. **Nothing from the VContainer conversion.** |
| 2 — plugins imported | 3 | `HapticTypes` — `Game.asmdef` was missing `MoreMountains.NiceVibrations.Haptics` |
| 3 — asmdef fixed | 2 | `TestMode` ambiguous in `TestsToolWindow.cs` (test-framework 1.7.0 vs the 1.3.9 the template targeted) |
| 4 — alias added | 0 | — |

Asset import was clean throughout: no missing scripts, no broken prefab references, no shader
errors, no GUID collisions between `Assets/Plugins/` and the package cache.

Remaining warnings are all pre-existing upstream code, none from the port: three unused events in
`NullableInputDevice`, a `CS4014` fire-and-forget in `SceneLoader`, a deprecated
`ParticleSystem.startColor`, and six obsolete-API warnings inside NiceVibrations itself.

Not verified: nothing has been run in Play Mode. The container is proven to compile and wire up
statically, but the boot flow has not been executed — that needs an Addressables content build first.
