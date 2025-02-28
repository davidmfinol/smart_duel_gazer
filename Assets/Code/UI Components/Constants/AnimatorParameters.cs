using UnityEngine;

namespace Code.UI_Components.Constants
{
    public static class AnimatorParameters
    {
        #region Model Animations

        public static readonly int SummoningTrigger = Animator.StringToHash("SummoningTrigger");
        public static readonly int DeathTrigger = Animator.StringToHash("Death");
        public static readonly int ActivateSpellOrTrapTrigger = Animator.StringToHash("ActivateSpellTrap");
        public static readonly int ReturnSpellTrapToFaceDown = Animator.StringToHash("ReturnToFaceDown");
        public static readonly int RemoveSetCardTrigger = Animator.StringToHash("RemoveSetCard");
        public static readonly int PlayMonsterAttack1Trigger = Animator.StringToHash("Attack1");
        public static readonly int TakeDamageTrigger = Animator.StringToHash("TakeDamage");
        public static readonly int PickUpPlayfieldTrigger = Animator.StringToHash("PickUpPlayfield");
        public static readonly int FadeInSetCardTrigger = Animator.StringToHash("FadeIn");
        public static readonly int HideSetCardTrigger = Animator.StringToHash("HideSetMonster");
        public static readonly int PlayfieldActivationTrigger = Animator.StringToHash("ActivatePlayfield");

        public static readonly int DefenceBool = Animator.StringToHash("IsDefence");
        public static readonly int RotatePlatformBool = Animator.StringToHash("Rotate");
        public static readonly int MainMenuRotateBool = Animator.StringToHash("MainMenuRotate");
        public static readonly int AllowDestroyBool = Animator.StringToHash("AllowDestroy");
        public static readonly int ShowSetCardImageBool = Animator.StringToHash("ShowCardImage");

        public static readonly int PlatformDoubleSpeedFloat = Animator.StringToHash("DoubleSpeed");

        #endregion

        #region UI Animations

        public static readonly int ActivatePlayfieldTrigger = Animator.StringToHash("ActivatePlayfield");
        public static readonly int RemovePlayfieldTrigger = Animator.StringToHash("RemovePlayfield");

        public static readonly int OpenPlayfieldMenuBool = Animator.StringToHash("OpenPlayfieldMenu");
        public static readonly int PrefabMenuBool = Animator.StringToHash("PrefabMenuEnter");

        #endregion
    }
}