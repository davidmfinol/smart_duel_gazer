using Code.Core.SmartDuelServer.Entities.EventData.RoomEvents;
using Code.Features.SpeedDuel.Models;

namespace Code.Features.SpeedDuel.UseCases
{
    public interface ICreatePlayerStateUseCase
    {
        PlayerState Execute(Duelist duelist, bool isOpponent);
    }

    public class CreatePlayerStateUseCase : ICreatePlayerStateUseCase
    {
        private const string UserPlayMatZonesPath = "UserPlayMat/Zones";
        private const string OpponentPlayMatZonesPath = "OpponentPlayMat/Zones";
        
        public PlayerState Execute(Duelist duelist, bool isOpponent)
        {
            var playMatZonesPath = isOpponent ? OpponentPlayMatZonesPath : UserPlayMatZonesPath;

            var playerState = new PlayerState(
                duelist.Id,
                isOpponent,
                playMatZonesPath,
                duelist.HandZone,
                duelist.FieldZone,
                duelist.MainMonsterZone1,
                duelist.MainMonsterZone2,
                duelist.MainMonsterZone3,
                duelist.GraveyardZone,
                duelist.BanishedZone,
                duelist.ExtraDeckZone,
                duelist.SpellTrapZone1,
                duelist.SpellTrapZone2,
                duelist.SpellTrapZone3,
                duelist.DeckZone,
                duelist.SkillZone
            );

            return playerState;
        }
    }
}