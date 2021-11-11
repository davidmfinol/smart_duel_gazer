using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using Code.Features.SpeedDuel.Models.Zones;

namespace Code.Features.SpeedDuel.UseCases.CardDeclare
{
    public interface ICardDeclareUseCase
    {
        void Execute(Zone zone);
    }

    public class CardDeclareUseCase : ICardDeclareUseCase
    {
        private const string Tag = "CardDeclareUseCase";
        
        private readonly IDataManager _dataManager;
        private readonly IAppLogger _logger;
        
        public CardDeclareUseCase(
            IDataManager dataManager,
            IAppLogger appLogger)
        {
            _dataManager = dataManager;
            _logger = appLogger;
        }

        public void Execute(Zone zone)
        {
            _logger.Log(Tag, $"Execute(zone: {zone.ZoneType})");

            if (!(zone is SingleCardZone singleCardZone)) return;

            var activateEffectParticles = _dataManager.GetGameObject(GameObjectKeys.ActivateEffectParticlesKey);
            activateEffectParticles.transform.position = singleCardZone.MonsterModel.transform.position;
            activateEffectParticles.SetActive(true);
        }
    }
}