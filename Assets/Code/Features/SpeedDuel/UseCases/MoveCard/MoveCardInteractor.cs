using System.Collections.Generic;
using System.Linq;
using Code.Core.SmartDuelServer.Entities.EventData.CardEvents;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using UnityEngine;

namespace Code.Features.SpeedDuel.UseCases.MoveCard
{
    public interface IMoveCardInteractor
    {
        PlayerState Execute(PlayerState playerState, PlayCard card, CardPosition position, Zone newZone = null,
            GameObject speedDuelField = null);
    }

    public class MoveCardInteractor : IMoveCardInteractor
    {
        private readonly IMoveCardToNewZoneUseCase _moveCardToNewZoneUseCase;
        private readonly IUpdateCardPositionUseCase _updateCardPositionUseCase;
        private readonly IRemoveCardUseCase _removeCardUseCase;

        public MoveCardInteractor(
            IMoveCardToNewZoneUseCase moveCardToNewZoneUseCase,
            IUpdateCardPositionUseCase updateCardPositionUseCase,
            IRemoveCardUseCase removeCardUseCase)
        {
            _moveCardToNewZoneUseCase = moveCardToNewZoneUseCase;
            _updateCardPositionUseCase = updateCardPositionUseCase;
            _removeCardUseCase = removeCardUseCase;
        }

        public PlayerState Execute(PlayerState playerState, PlayCard card, CardPosition position, Zone newZone = null,
            GameObject speedDuelField = null)
        {
            var playerZones = playerState.GetZones().ToList();
            var oldZone = playerZones.First(zone => zone.ZoneType == card.ZoneType);

            IEnumerable<Zone> updatedZones;
            if (position == CardPosition.Destroy)
            {
                updatedZones = _removeCardUseCase.Execute(card, playerZones, oldZone);
            }
            else if (newZone == null || newZone.ZoneType == oldZone.ZoneType)
            {
                updatedZones =
                    _updateCardPositionUseCase.Execute(playerState, card, position, playerZones, oldZone, speedDuelField);
            }
            else
            {
                updatedZones = _moveCardToNewZoneUseCase.Execute(playerState, card, position, playerZones, oldZone, newZone,
                    speedDuelField);
            }

            return playerState.CopyWith(updatedZones.ToList());
        }
    }
}