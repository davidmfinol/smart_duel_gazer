using Code.Core.DataManager;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IPlayCardInteractor
    {
        Zone Execute(PlayerState playerState, SingleCardZone zone, PlayCard updatedCard, GameObject speedDuelField);
    }

    public class PlayCardInteractor : IPlayCardInteractor
    {
        private readonly IDataManager _dataManager;
        private readonly IPlayCardImageUseCase _playCardImageUseCase;
        private readonly IPlayCardModelUseCase _playCardModelUseCase;

        public PlayCardInteractor(
            IDataManager dataManager,
            IPlayCardImageUseCase playCardImageUseCase,
            IPlayCardModelUseCase playCardModelUseCase)
        {
            _dataManager = dataManager;
            _playCardImageUseCase = playCardImageUseCase;
            _playCardModelUseCase = playCardModelUseCase;
        }

        public Zone Execute(PlayerState playerState, SingleCardZone zone, PlayCard updatedCard, GameObject speedDuelField)
        {
            var playMatZonePath = $"{playerState.PlayMatZonesPath}/{updatedCard.ZoneType.ToString()}";
            var playMatZone = speedDuelField.transform.Find(playMatZonePath);

            var monsterModel = _dataManager.GetCardModel(updatedCard.Id);
            if (!monsterModel)
            {
                return _playCardImageUseCase.Execute(zone, updatedCard, playMatZone);
            }

            monsterModel.SetActive(true);
            return _playCardModelUseCase.Execute(zone, updatedCard, playMatZone, monsterModel, speedDuelField);
        }
    }
}