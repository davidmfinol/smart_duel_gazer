namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public enum ModelEvent
    {
        SummonMonster,
        ChangeMonsterVisibility,
        DestroyMonster,
        MonsterDestruction,
        Attack,
        RevealSetMonster,
    }

    public enum SetCardEvent
    {
        SummonSetCard,
        ShowSetCard,
        HideSetMonster,
        DestroySetMonster,
        SpellTrapActivate,
        SetCardRemove,
        ReturnToFaceDown
    }
}
