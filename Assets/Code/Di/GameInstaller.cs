using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.DuelRooms;
using Code.Core.DataManager.GameObjects;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.DataManager.Textures;
using Code.Core.Dialog;
using Code.Core.Models.ModelComponentsManager;
using Code.Core.Models.ModelEventsHandler;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.Storage.Connection;
using Code.Core.Storage.DuelRoom;
using Code.Core.Storage.GameObject;
using Code.Core.Storage.Texture;
using Code.Core.YGOProDeck;
using Code.Features.Connection.Helpers;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using Code.Features.SpeedDuel.UseCases;
using Code.Features.SpeedDuel.UseCases.MoveCard;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
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

            // API providers
            Container.Bind<IYgoProDeckApiProvider>().To<YgoProDeckApiProvider>().AsSingle();

            // Storage providers
            Container.Bind<IPlayerPrefsProvider>().To<PlayerPrefsProvider>().AsSingle();
            Container.Bind<IResourcesProvider>().To<ResourcesProvider>().AsSingle();
            Container.Bind<IConnectionStorageProvider>().To<ConnectionStorageProvider>().AsSingle();
            Container.Bind<IGameObjectStorageProvider>().To<GameObjectStorageProvider>().AsSingle();
            Container.Bind<ITextureStorageProvider>().To<TextureStorageProvider>().AsSingle();
            Container.Bind<IDuelRoomStorageProvider>().To<DuelRoomStorageProvider>().AsSingle();

            Container.Bind<ISmartDuelServer>().To<SmartDuelServer>().AsSingle();

            Container.Bind<ModelEventHandler>().AsSingle();
            Container.BindFactory<GameObject, ModelComponentsManager, ModelComponentsManager.Factory>()
                .FromFactory<PrefabFactory<ModelComponentsManager>>();

            // Use cases
            Container.Bind<IGetTransformedGameObjectUseCase>().To<GetTransformedGameObjectUseCase>().AsSingle();
            Container.Bind<IRecycleGameObjectUseCase>().To<RecycleGameObjectUseCase>().AsSingle();

            #endregion

            #region Features

            Container.Bind<ConnectionFormValidators>().AsSingle();

            Container.BindFactory<GameObject, DestructionParticles, DestructionParticles.Factory>()
                .FromFactory<PrefabFactory<DestructionParticles>>();
            Container.BindFactory<GameObject, SetCard, SetCard.Factory>()
                .FromFactory<PrefabFactory<SetCard>>();

            // Use cases
            Container.Bind<ICreatePlayCardUseCase>().To<CreatePlayCardUseCase>().AsSingle();
            Container.Bind<ICreatePlayerStateUseCase>().To<CreatePlayerStateUseCase>().AsSingle();
            Container.Bind<IMoveCardInteractor>().To<MoveCardInteractor>().AsSingle();
            Container.Bind<IMoveCardToNewZoneUseCase>().To<MoveCardToNewZoneUseCase>().AsSingle();
            Container.Bind<IUpdateCardPositionUseCase>().To<UpdateCardPositionUseCase>().AsSingle();
            Container.Bind<IPlayCardInteractor>().To<PlayCardInteractor>().AsSingle();
            Container.Bind<IPlayCardImageUseCase>().To<PlayCardImageUseCase>().AsSingle();
            Container.Bind<IPlayCardModelUseCase>().To<PlayCardModelUseCase>().AsSingle();
            Container.Bind<IRemoveCardUseCase>().To<RemoveCardUseCase>().AsSingle();
            Container.Bind<IHandlePlayCardModelEventsUseCase>().To<HandlePlayCardModelEventsUseCase>().AsSingle();

            #endregion

            #region Wrappers

            Container.Bind<IWebSocketFactory>().To<WebSocketFactory>().AsSingle();
            Container.Bind<IWebSocketProvider>().To<WebSocketProvider>().AsTransient();
            Container.Bind<SocketIO>().FromFactory<SocketIOFactory>();

            #endregion
        }
    }
}