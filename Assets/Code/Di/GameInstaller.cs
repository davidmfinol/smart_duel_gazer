using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.DuelRooms;
using Code.Core.DataManager.GameObjects;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.DataManager.Textures;
using Code.Core.DataManager.UserSettings;
using Code.Core.Dialog;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.Storage.Connection;
using Code.Core.Storage.DuelRoom;
using Code.Core.Storage.GameObject;
using Code.Core.Storage.Texture;
using Code.Core.Storage.UserSettings;
using Code.Core.YGOProDeck;
using Code.Features.Connection.Helpers;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using Code.Features.SpeedDuel.UseCases;
using Code.Features.SpeedDuel.UseCases.MoveCard;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
using Code.Wrappers.WrapperLogger;
using Code.Wrappers.WrapperPlayerPrefs;
using Code.Wrappers.WrapperResources;
using Code.Wrappers.WrapperWebSocket;
using Dpoch.SocketIO;
using UnityEngine;
using Zenject;

namespace Code.Di
{
    public class GameInstaller : MonoInstaller
    {
        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            #region Core

            // Services
            Container.Bind<IDialogService>().To<DialogService>().AsSingle();
            Container.Bind<IScreenService>().To<ScreenService>().AsSingle();
            Container.Bind<INavigationService>().To<NavigationService>().AsSingle();

            // Config
            Container.Bind<IDelayProvider>().To<DelayProvider>().AsSingle();

            // Data managers
            Container.Bind<IDataManager>().To<DataManager>().AsSingle();
            Container.Bind<IConnectionDataManager>().To<ConnectionDataManager>().AsSingle();
            Container.Bind<IGameObjectDataManager>().To<GameObjectDataManager>().AsSingle();
            Container.Bind<ITextureDataManager>().To<TextureDataManager>().AsSingle();
            Container.Bind<IDuelRoomDataManager>().To<DuelRoomDataManager>().AsSingle();
            Container.Bind<IUserSettingsDataManager>().To<UserSettingsDataManager>().AsSingle();

            // Use cases
            Container.Bind<IGetTransformedGameObjectUseCase>().To<GetTransformedGameObjectUseCase>().AsSingle();
            Container.Bind<IRecycleGameObjectUseCase>().To<RecycleGameObjectUseCase>().AsSingle();

            // API providers
            Container.Bind<IYgoProDeckApiProvider>().To<YgoProDeckApiProvider>().AsSingle();

            // Storage providers
            Container.Bind<IPlayerPrefsProvider>().To<PlayerPrefsProvider>().AsSingle();
            Container.Bind<IResourcesProvider>().To<ResourcesProvider>().AsSingle();
            Container.Bind<IConnectionStorageProvider>().To<ConnectionStorageProvider>().AsSingle();
            Container.Bind<IGameObjectStorageProvider>().To<GameObjectStorageProvider>().AsSingle();
            Container.Bind<ITextureStorageProvider>().To<TextureStorageProvider>().AsSingle();
            Container.Bind<IDuelRoomStorageProvider>().To<DuelRoomStorageProvider>().AsSingle();
            Container.Bind<IUserSettingsStorageProvider>().To<UserSettingsStorageProvider>().AsSingle();

            // Smart duel server
            Container.Bind<ISmartDuelServer>().To<SmartDuelServer>().AsSingle();

            // Logger
            Container.Bind<IAppLogger>().To<AppLogger>().AsSingle();

            #endregion

            #region Features

            Container.Bind<ConnectionFormValidators>().AsSingle();

            // Event Handlers
            Container.Bind<ModelEventHandler>().AsSingle();
            Container.Bind<PlayfieldEventHandler>().AsSingle();
            Container.Bind<SetCardEventHandler>().AsSingle();

            // Prefabs
            Container.BindFactory<GameObject, DestructionParticles, DestructionParticles.Factory>()
                .FromFactory<PrefabFactory<DestructionParticles>>();
            Container.BindFactory<GameObject, ModelComponentsManager, ModelComponentsManager.Factory>()
                .FromFactory<PrefabFactory<ModelComponentsManager>>();
            Container.BindFactory<GameObject, SetCard, SetCard.Factory>()
                .FromFactory<PrefabFactory<SetCard>>();

            // Use cases
            Container.Bind<ICreatePlayCardUseCase>().To<CreatePlayCardUseCase>().AsSingle();
            Container.Bind<ICreatePlayerStateUseCase>().To<CreatePlayerStateUseCase>().AsSingle();
            Container.Bind<IMoveCardInteractor>().To<MoveCardInteractor>().AsSingle();
            Container.Bind<IMoveCardToNewZoneUseCase>().To<MoveCardToNewZoneUseCase>().AsSingle();
            Container.Bind<IUpdateCardPositionUseCase>().To<UpdateCardPositionUseCase>().AsSingle();
            Container.Bind<IRemoveCardUseCase>().To<RemoveCardUseCase>().AsSingle();
            Container.Bind<IPlayCardInteractor>().To<PlayCardInteractor>().AsSingle();
            Container.Bind<IPlayCardImageUseCase>().To<PlayCardImageUseCase>().AsSingle();
            Container.Bind<IPlayCardModelUseCase>().To<PlayCardModelUseCase>().AsSingle();
            Container.Bind<IRemoveCardModelUseCase>().To<RemoveCardModelUseCase>().AsSingle();
            Container.Bind<IHandlePlayCardModelEventsUseCase>().To<HandlePlayCardModelEventsUseCase>().AsSingle();

            #endregion

            #region Wrappers

            Container.Bind<IWebSocketFactory>().To<WebSocketFactory>().AsSingle();
            Container.Bind<IWebSocketProvider>().To<WebSocketProvider>().AsTransient();
            Container.Bind<SocketIO>().FromFactory<SocketIOFactory>();

            Container.Bind<ILoggerProvider>().To<LoggerProvider>().AsSingle();

            #endregion
        }
    }
}