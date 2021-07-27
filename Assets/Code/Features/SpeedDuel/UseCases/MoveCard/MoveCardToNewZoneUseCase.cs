using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard
{
    public interface IMoveCardToNewZoneUseCase
    {
        IEnumerable<Zone> Execute(PlayerState playerState, PlayCard oldCard, CardPosition position, IEnumerable<Zone> playerZones,
            Zone oldZone, Zone newZone, GameObject speedDuelField);
    }

    public class MoveCardToNewZoneUseCase : IMoveCardToNewZoneUseCase
    {
        private readonly IPlayCardInteractor _playCardInteractor;
        private readonly IRemoveCardUseCase _removeCardUseCase;

        public MoveCardToNewZoneUseCase(
            IPlayCardInteractor playCardInteractor,
            IRemoveCardUseCase removeCardUseCase)
        {
            _playCardInteractor = playCardInteractor;
            _removeCardUseCase = removeCardUseCase;
        }

        public IEnumerable<Zone> Execute(PlayerState playerState, PlayCard oldCard, CardPosition position,
            IEnumerable<Zone> playerZones, Zone oldZone, Zone newZone, GameObject speedDuelField)
        {
            var updatedOldZone = RemoveCardFromZone(oldZone, oldCard);

            var updatedCard = oldCard.CopyWith(cardPosition: position, zoneType: newZone.ZoneType);
            var updatedNewZone = AddCardToZone(newZone, updatedCard, speedDuelField, playerState);

            var updatedZones = playerZones.ToList();
            updatedZones.Remove(oldZone);
            updatedZones.Remove(newZone);
            updatedZones.Add(updatedOldZone);
            updatedZones.Add(updatedNewZone);

            return updatedZones;
        }

        private Zone RemoveCardFromZone(Zone zone, PlayCard oldCard)
        {
            switch (zone)
            {
                case SingleCardZone singleCardZone:
                    return RemoveCardFromSingleCardZone(singleCardZone, oldCard);
                case MultiCardZone multiCardZone:
                {
                    var cards = multiCardZone.Cards;
                    cards.Remove(oldCard);
                    return multiCardZone.CopyWith(cards);
                }
                default:
                    return null;
            }
        }

        private Zone RemoveCardFromSingleCardZone(SingleCardZone zone, PlayCard oldCard)
        {
            return _removeCardUseCase.Execute(zone, oldCard);
        }

        private Zone AddCardToZone(Zone zone, PlayCard updatedCard, GameObject speedDuelField, PlayerState playerState)
        {
            switch (zone)
            {
                case SingleCardZone singleCardZone:
                    return AddCardToSingleCardZone(singleCardZone, updatedCard, speedDuelField, playerState);
                case MultiCardZone multiCardZone:
                {
                    var cards = multiCardZone.Cards;
                    cards.Add(updatedCard);
                    return multiCardZone.CopyWith(cards);
                }
                default:
                    return null;
            }
        }

        private Zone AddCardToSingleCardZone(SingleCardZone zone, PlayCard updatedCard, GameObject speedDuelField,
            PlayerState playerState)
        {
            return _playCardInteractor.Execute(playerState, zone, updatedCard, speedDuelField);
        }
    }
}