namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public enum PlayfieldEvent
    {
        Transparency,
        Scale
    }
    
    public enum ModelEvent
    {
        SummonMonster,
        ChangeMonsterVisibility,
        DestroyMonster,
        MonsterDestruction,
        AttackDeclaration,
        RevealSetMonsterModel,
        DamageStep
    }

    public enum SetCardEvent
    {
        SummonSetCard,
        RevealSetCardImage,
        HideSetCardImage,
        DestroySetMonster,
        SpellTrapActivate,
        SetCardRemove,
        ReturnToFaceDown,
        Hurt
    }
}
