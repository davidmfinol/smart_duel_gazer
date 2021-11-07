using Code.Core.DataManager;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents
{
    public interface IPlayCardInteractor
    {
        Zone Execute(PlayerState playerState, SingleCardZone zone, PlayCard card, GameObject speedDuelField);
    }

    public class PlayCardInteractor : IPlayCardInteractor
    {
        private readonly IDataManager _dataManager;
        private readonly IPlayCardImageUseCase _playCardImageUseCase;
        private readonly IPlayCardModelUseCase _playCardModelUseCase;

        #region Constructor

        public PlayCardInteractor(
            IDataManager dataManager,
            IPlayCardImageUseCase playCardImageUseCase,
            IPlayCardModelUseCase playCardModelUseCase)
        {
            _dataManager = dataManager;
            _playCardImageUseCase = playCardImageUseCase;
            _playCardModelUseCase = playCardModelUseCase;
        }

        #endregion

        public Zone Execute(PlayerState playerState, SingleCardZone zone, PlayCard card, GameObject speedDuelField)
        {
            var playMatZonePath = $"{playerState.PlayMatZonesPath}/{card.ZoneType.ToString()}";
            var playMatZone = speedDuelField.transform.Find(playMatZonePath);

            var monsterModel = _dataManager.GetCardModel(card.YugiohCard.Id);
            if (!monsterModel)
            {
                return _playCardImageUseCase.Execute(zone, card, playMatZone);
            }

            monsterModel.SetActive(true);
            return _playCardModelUseCase.Execute(zone, card, playMatZone, monsterModel, speedDuelField);
        }
    }
}