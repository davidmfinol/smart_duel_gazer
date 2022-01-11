using Code.Core.Config.Providers;
using Code.Core.General.Extensions;
using UnityEngine;

namespace Code.Core.DataManager.GameObjects.UseCases
{
    public interface IRecycleGameObjectUseCase
    {
        void Execute(GameObject model);
    }
    
    public class RecycleGameObjectUseCase : IRecycleGameObjectUseCase
    {
        private const int RemoveCardDurationInMs = 7000;
        
        private readonly IDelayProvider _delayProvider;

        public RecycleGameObjectUseCase(
            IDelayProvider delayProvider)
        {
            _delayProvider = delayProvider;
        }
        
        public async void Execute(GameObject model)
        {
            await _delayProvider.Wait(RemoveCardDurationInMs);
            
            model.SetActive(false);
        }
    }
}