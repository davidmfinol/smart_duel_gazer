using System.Collections.Generic;
using System.Linq;
using Code.Features.SpeedDuel.Models;
using Code.Features.SpeedDuel.Models.Zones;
using Code.Features.SpeedDuel.UseCases.MoveCard.ModelsAndEvents;

namespace Code.Features.SpeedDuel.UseCases.MoveCard
{
    public interface IRemoveCardUseCase
    {
        IEnumerable<Zone> Execute(PlayCard oldCard, IEnumerable<Zone> playerZones, Zone oldZone);
    }

    public class RemoveCardUseCase : IRemoveCardUseCase
    {
        private readonly IRemoveCardModelUseCase _removeCardModelUseCase;

        public RemoveCardUseCase(
            IRemoveCardModelUseCase removeCardModelUseCase)
        {
            _removeCardModelUseCase = removeCardModelUseCase;
        }

        public IEnumerable<Zone> Execute(PlayCard oldCard, IEnumerable<Zone> playerZones, Zone oldZone)
        {
            var updatedOldZone = RemoveCardFromZone(oldZone, oldCard);

            var updatedZones = playerZones.ToList();
            updatedZones.Remove(oldZone);
            updatedZones.Add(updatedOldZone);

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
            return _removeCardModelUseCase.Execute(zone, oldCard);
        }
    }
}