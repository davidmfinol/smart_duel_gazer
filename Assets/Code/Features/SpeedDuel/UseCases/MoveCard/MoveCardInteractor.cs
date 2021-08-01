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

            return UpdatePlayerState(playerState, updatedZones);
        }

        private static PlayerState UpdatePlayerState(PlayerState playerState, IEnumerable<Zone> updatedZones)
        {
            var zones = updatedZones.ToList();

            return playerState.CopyWith(
                zones.First(z => z.ZoneType == ZoneType.Hand) as MultiCardZone,
                zones.First(z => z.ZoneType == ZoneType.Field) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.MainMonster1) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.MainMonster2) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.MainMonster3) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.Graveyard) as MultiCardZone,
                zones.First(z => z.ZoneType == ZoneType.Banished) as MultiCardZone,
                zones.First(z => z.ZoneType == ZoneType.ExtraDeck) as MultiCardZone,
                zones.First(z => z.ZoneType == ZoneType.SpellTrap1) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.SpellTrap2) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.SpellTrap3) as SingleCardZone,
                zones.First(z => z.ZoneType == ZoneType.Deck) as MultiCardZone
            );
        }
    }
}