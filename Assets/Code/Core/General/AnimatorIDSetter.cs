using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General
{
    public static class AnimatorIDSetter
    {
        #region Model Animations
            
        public static readonly int Animator_Summoning_Trigger = Animator.StringToHash("SummoningTrigger");
        public static readonly int Animator_Activate_Spell_Or_Trap = Animator.StringToHash("ActivateSpellTrap");
        public static readonly int Animator_Show_Set_Monster = Animator.StringToHash("ShowSetMonster");
        public static readonly int Death_Trigger = Animator.StringToHash("Death");

        #endregion

        #region UI Animations

        public static readonly int Animator_Remove_Playfield = Animator.StringToHash("RemovePlayfield");
        public static readonly int Animator_Open_Playfield_Menu = Animator.StringToHash("OpenPlayfieldMenu");
        
        #endregion
    }
}
