using AssemblyCSharp.Assets.Code.Core.Config.Interface.Providers;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using Code.Core.DataManager.Interface.GameObject.UseCases;

namespace Code.Core.DataManager.Impl.GameObject.UseCases
{
    public class RecycleGameObjectUseCase : IRecycleGameObjectUseCase
    {
        private const int RemoveCardDurationInMs = 7000;

        private readonly IDataManager _dataManager;
        private readonly IDelayProvider _delayProvider;

        public RecycleGameObjectUseCase(
            IDataManager dataManager,
            IDelayProvider delayProvider)
        {
            _dataManager = dataManager;
            _delayProvider = delayProvider;
        }
        
        public async void Execute(string key, UnityEngine.GameObject model)
        {
            await _delayProvider.Wait(RemoveCardDurationInMs);
            
            model.SetActive(false);

            _dataManager.SaveGameObject(key.RemoveCloneSuffix(), model);
        }
    }
}