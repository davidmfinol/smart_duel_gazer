﻿using AssemblyCSharp.Assets.Code.Core.Config.Interface.Providers;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Impl.Texture;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.Dialog.Impl;
using AssemblyCSharp.Assets.Code.Core.Dialog.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelComponentsManager;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Navigation.Impl;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Impl;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.GameObject;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Impl;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.PlayerPrefs.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Impl;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Providers.Resources.Interface;
using AssemblyCSharp.Assets.Code.Core.Storage.Impl.Texture;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Connection;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.GameObject;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.Texture;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using AssemblyCSharp.Assets.Code.Features.Connection.Helpers;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.ParticleSystems.Scripts;
using AssemblyCSharp.Assets.Code.Features.SpeedDuel.PrefabManager.Prefabs.SetCard.Scripts;
using Code.Core.Config.Impl.Providers;
using Code.Core.DataManager.DuelRoom;
using Code.Core.SmartDuelServer.Impl;
using Code.Core.SmartDuelServer.Interface;
using Code.Core.Storage.DuelRoom;
using Code.Wrappers.WrapperWebSocket.Impl;
using Code.Wrappers.WrapperWebSocket.Interface;
using Dpoch.SocketIO;
using UnityEngine;
using Zenject;

namespace Code.Di
{
    public class GameInstaller : MonoInstaller
    {
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
            Container.Bind<IYGOProDeckApiProvider>().To<YGOProDeckApiProvider>().AsSingle();

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

            #endregion

            #region Features

            Container.Bind<ConnectionFormValidators>().AsSingle();

            Container.BindFactory<GameObject, DestructionParticles, DestructionParticles.Factory>()
                .FromFactory<PrefabFactory<DestructionParticles>>();
            Container.BindFactory<GameObject, SetCard, SetCard.Factory>()
                .FromFactory<PrefabFactory<SetCard>>();

            #endregion

            #region Wrappers

            Container.Bind<IWebSocketFactory>().To<WebSocketFactory>().AsSingle();
            Container.Bind<IWebSocketProvider>().To<WebSocketProvider>().AsTransient();
            Container.Bind<SocketIO>().FromFactory<SocketIOFactory>();

            #endregion
        }
    }
}