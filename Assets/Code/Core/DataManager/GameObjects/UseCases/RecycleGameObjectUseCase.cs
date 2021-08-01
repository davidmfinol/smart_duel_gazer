using Code.Core.Config.Providers;
using Code.Core.General.Extensions;
using UnityEngine;

namespace Code.Core.DataManager.GameObjects.UseCases
{
    public interface IRecycleGameObjectUseCase
    {
        void Execute(string key, GameObject model);
    }
    
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
        
        public async void Execute(string key, GameObject model)
        {
            await _delayProvider.Wait(RemoveCardDurationInMs);
            
            model.SetActive(false);

            _dataManager.SaveGameObject(key.RemoveCloneSuffix(), model);
        }
    }
}