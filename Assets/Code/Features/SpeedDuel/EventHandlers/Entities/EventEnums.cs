namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public enum PlayfieldEvent
    {
        Hide,
        Flip,
        Rotate,
        Scale
    }
    
    public enum ModelEvent
    {
        SummonMonster,
        ChangeMonsterVisibility,
        DestroyMonster,
        MonsterDestruction,
        Attack,
        RevealSetMonsterModel,
    }

    public enum SetCardEvent
    {
        SummonSetCard,
        RevealSetCardImage,
        HideSetCardImage,
        DestroySetMonster,
        SpellTrapActivate,
        SetCardRemove,
        ReturnToFaceDown
    }
}
