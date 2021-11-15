using Code.Core.DataManager;
using Code.Core.DataManager.GameObjects.Entities;
using Code.Core.Logger;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface IDeclareCardUseCase
    {
        void Execute(Zone zone);
    }

    public class DeclareCardUseCase : IDeclareCardUseCase
    {
        private const string Tag = "DeclareCardUseCase";

        private readonly IDataManager _dataManager;
        private readonly IAppLogger _logger;

        public DeclareCardUseCase(
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

            var targetPosition = singleCardZone.MonsterModel == null
                ? singleCardZone.SetCardModel.transform.position
                : singleCardZone.MonsterModel.transform.position;

            var activateEffectParticles = _dataManager.GetGameObject(GameObjectKeys.ActivateEffectParticlesKey);
            activateEffectParticles.transform.position = targetPosition;
            activateEffectParticles.SetActive(true);
        }
    }
}