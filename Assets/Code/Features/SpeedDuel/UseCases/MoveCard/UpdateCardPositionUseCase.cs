using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Interface.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard
{
    public interface IUpdateCardPositionUseCase
    {
        IEnumerable<Zone> Execute(PlayerState playerState, PlayCard oldCard, CardPosition position, IEnumerable<Zone> playerZones,
            Zone oldZone, GameObject speedDuelField);
    }

    public class UpdateCardPositionUseCase : IUpdateCardPositionUseCase
    {
        private readonly IPlayCardInteractor _playCardInteractor;

        public UpdateCardPositionUseCase(
            IPlayCardInteractor playCardInteractor)
        {
            _playCardInteractor = playCardInteractor;
        }

        public IEnumerable<Zone> Execute(PlayerState playerState, PlayCard oldCard, CardPosition position,
            IEnumerable<Zone> playerZones, Zone oldZone, GameObject speedDuelField)
        {
            var updatedCard = oldCard.CopyWith(cardPosition: position);
            var updatedOldZone = UpdateCardInZone(oldZone, oldCard, updatedCard, speedDuelField, playerState);

            var updatedZones = playerZones.ToList();
            updatedZones.Remove(oldZone);
            updatedZones.Add(updatedOldZone);

            return updatedZones;
        }

        private Zone UpdateCardInZone(Zone zone, PlayCard oldCard, PlayCard updatedCard, GameObject speedDuelField,
            PlayerState playerState)
        {
            switch (zone)
            {
                case SingleCardZone singleCardZone:
                    return UpdateCardInSingleCardZone(singleCardZone, updatedCard, speedDuelField, playerState);
                case MultiCardZone multiCardZone:
                {
                    var cards = multiCardZone.Cards;
                    cards.Remove(oldCard);
                    cards.Add(updatedCard);
                    return multiCardZone.CopyWith(cards);
                }
                default:
                    return null;
            }
        }

        private Zone UpdateCardInSingleCardZone(SingleCardZone zone, PlayCard updatedCard, GameObject speedDuelField,
            PlayerState playerState)
        {
            return _playCardInteractor.Execute(playerState, zone, updatedCard, speedDuelField);
        }
    }
}