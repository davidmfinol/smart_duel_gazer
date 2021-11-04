using UnityEngine;

namespace Code.Features.SpeedDuel.EventHandlers.Entities
{
    public abstract class ModelActionEventArgs
    {
    }

    public class ModelActionBoolEvent : ModelActionEventArgs
    {
        public bool State { get; set; }
    }

    public class ModelActionAttackEvent : ModelActionEventArgs
    {
        public GameObject TargetMonster { get; set; }
        public Transform PlayfieldTargetTransform { get; set; }
        public bool IsAttackingMonster { get; set; }
    }
}