using Code.Features.SpeedDuel.UseCases.CardBattle;
using Code.Core.Config.Entities;
using Code.Core.Config.Providers;
using Code.Core.DataManager;
using Code.Core.DataManager.Connections;
using Code.Core.DataManager.DuelRooms;
using Code.Core.DataManager.GameObjects;
using Code.Core.DataManager.GameObjects.UseCases;
using Code.Core.DataManager.Settings;
using Code.Core.DataManager.Textures;
using Code.Core.Dialog;
using Code.Core.Localization;
using Code.Core.Logger;
using Code.Core.Navigation;
using Code.Core.Screen;
using Code.Core.SmartDuelServer;
using Code.Core.Storage.Connections;
using Code.Core.Storage.DuelRooms;
using Code.Core.Storage.GameObjects;
using Code.Core.Storage.Settings;
using Code.Core.Storage.Textures;
using Code.Core.YGOProDeck;
using Code.Features.Connection;
using Code.Features.Connection.Helpers;
using Code.Features.DuelRoom;
using Code.Features.Onboarding;
using Code.Features.SpeedDuel;
using Code.Features.SpeedDuel.EventHandlers;
using Code.Features.SpeedDuel.PrefabManager.ModelComponentsManager;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using Code.Features.SpeedDuel.UseCases;
using Code.Features.SpeedDuel.UseCases.MoveCard;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
using Code.Wrappers.WrapperDialog;
using Code.Wrappers.WrapperFirebase;
using Code.Wrappers.WrapperLogger;
using Code.Wrappers.WrapperPlayerPrefs;
using Code.Wrappers.WrapperResources;
using Code.Wrappers.WrapperToast;
using Code.Wrappers.WrapperWebSocket;
using Dpoch.SocketIO;
using UnityEngine;
using Zenject;
using Code.Wrappers.WrapperNetworkConnection;
using UnityEngine.Localization;
using UnityEngine.Localization.Tables;
using Code.Features.SpeedDuel.UseCases.CardDeclare;

namespace Code.Di
{
    public class GameInstaller : MonoInstaller
    {
        [SerializeField]
        private LocalizedStringTable localizedStringTable;
        
        // ReSharper disable Unity.PerformanceAnalysis
        public override void InstallBindings()
        {
            #region Core

            // Services
            Container.Bind<IDialogService>().To<DialogService>().AsSingle();
            Container.Bind<IScreenService>().To<ScreenService>().AsSingle();
            Container.Bind<INavigationService>().To<NavigationService>().AsSingle();

            // Config
            Container.Bind<IAppConfig>().FromInstance(new AppConfig());
            Container.Bind<IDelayProvider>().To<DelayProvider>().AsSingle();
            Container.Bind<ITimeProvider>().To<TimeProvider>().AsSingle();

            // Data managers
            Container.Bind<IDataManager>().To<DataManager>().AsSingle();
            Container.Bind<IConnectionDataManager>().To<ConnectionDataManager>().AsSingle();
            Container.Bind<IGameObjectDataManager>().To<GameObjectDataManager>().AsSingle();
            Container.Bind<ITextureDataManager>().To<TextureDataManager>().AsSingle();
            Container.Bind<IDuelRoomDataManager>().To<DuelRoomDataManager>().AsSingle();
            Container.Bind<ISettingsDataManager>().To<SettingsDataManager>().AsSingle();

            // Use cases
            Container.Bind<IGetTransformedGameObjectUseCase>().To<GetTransformedGameObjectUseCase>().AsSingle();
            Container.Bind<IRecycleGameObjectUseCase>().To<RecycleGameObjectUseCase>().AsSingle();

            // API providers
            Container.Bind<IYgoProDeckApiProvider>().To<YgoProDeckApiProvider>().AsSingle();

            // Storage providers
            Container.Bind<IConnectionStorageProvider>().To<ConnectionStorageProvider>().AsSingle();
            Container.Bind<IGameObjectStorageProvider>().To<GameObjectStorageProvider>().AsSingle();
            Container.Bind<ITextureStorageProvider>().To<TextureStorageProvider>().AsSingle();
            Container.Bind<IDuelRoomStorageProvider>().To<DuelRoomStorageProvider>().AsSingle();
            Container.Bind<ISettingsStorageProvider>().To<SettingsStorageProvider>().AsSingle();

            // Smart duel server
            Container.Bind<ISmartDuelServer>().To<SmartDuelServer>().AsSingle();
            
            // String provider
            var stringTable =  localizedStringTable.GetTable();
            Container.Bind<StringTable>().FromInstance(stringTable);
            Container.Bind<IStringProvider>().To<StringProvider>().AsSingle();

            // Logger
            Container.Bind<IAppLogger>().To<AppLogger>().AsSingle();

            #endregion

            #region Features

            Container.Bind<ConnectionFormValidators>().AsSingle();

            // ViewModels
            Container.Bind<OnboardingViewModel>().AsTransient();
            Container.Bind<ConnectionViewModel>().AsTransient();
            Container.Bind<SpeedDuelViewModel>().AsTransient();
            Container.Bind<DuelRoomViewModel>().AsTransient();

            // Event Handlers
            Container.Bind<IModelEventHandler>().To<ModelEventHandler>().AsSingle();
            Container.Bind<IPlayfieldEventHandler>().To<PlayfieldEventHandler>().AsSingle();
            Container.Bind<ISetCardEventHandler>().To<SetCardEventHandler>().AsSingle();
            Container.Bind<IEndOfDuelUseCase>().To<EndOfDuelUseCase>().AsSingle();

            // Prefabs
            Container.BindFactory<GameObject, DestructionParticles, DestructionParticles.Factory>()
                .FromFactory<PrefabFactory<DestructionParticles>>();
            Container.BindFactory<GameObject, ModelComponentsManager, ModelComponentsManager.Factory>()
                .FromFactory<PrefabFactory<ModelComponentsManager>>();
            Container.BindFactory<GameObject, SetCard, SetCard.Factory>()
                .FromFactory<PrefabFactory<SetCard>>();
            Container.BindFactory<GameObject, PlayfieldComponentsManager, PlayfieldComponentsManager.Factory>()
                .FromFactory<PrefabFactory<PlayfieldComponentsManager>>();
            Container.BindFactory<GameObject, ActivateEffectParticles, ActivateEffectParticles.Factory>()
                .FromFactory<PrefabFactory<ActivateEffectParticles>>();

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
            Container.Bind<IMonsterBattleInteractor>().To<MonsterBattleInteractor>().AsSingle();
            Container.Bind<IMonsterZoneBattleUseCase>().To<MonsterZoneBattleUseCase>().AsSingle();
            Container.Bind<IDirectAttackUseCase>().To<DirectAttackUseCase>().AsSingle();
            Container.Bind<ICardDeclareUseCase>().To<CardDeclareUseCase>().AsSingle();

            #endregion

            #region Wrappers

            Container.Bind<IDialogProvider>().To<DialogProvider>().AsSingle();
            Container.Bind<IFirebaseInitializer>().To<FirebaseInitializer>().AsSingle();
            Container.Bind<ILoggerProvider>().To<LoggerProvider>().AsSingle();
            Container.Bind<IPlayerPrefsProvider>().To<PlayerPrefsProvider>().AsSingle();
            Container.Bind<IResourcesProvider>().To<ResourcesProvider>().AsSingle();
            Container.Bind<IToastProvider>().To<ToastProvider>().AsSingle();
            Container.Bind<IWebSocketFactory>().To<WebSocketFactory>().AsSingle();
            Container.Bind<IWebSocketProvider>().To<WebSocketProvider>().AsTransient();
            Container.Bind<SocketIO>().FromFactory<SocketIOFactory>();
            Container.Bind<INetworkConnectionProvider>().To<NetworkConnectionProvider>().AsSingle();

            #endregion
        }
    }
}