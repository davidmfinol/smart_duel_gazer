using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General.Statics
{
    public static class AnimatorParams
    {
        #region Model Animations
            
        public static readonly int Summoning_Trigger = Animator.StringToHash("SummoningTrigger");
        public static readonly int Activate_Spell_Or_Trap_Trigger = Animator.StringToHash("ActivateSpellTrap");
        public static readonly int Remove_Spell_Or_Trap_Trigger = Animator.StringToHash("RemoveSpellTrap");
        public static readonly int Reveal_Set_Monster_Trigger = Animator.StringToHash("RevealSetMonster");
        public static readonly int Play_Monster_Attack_1_Trigger = Animator.StringToHash("Attack1");
        public static readonly int Death_Trigger = Animator.StringToHash("Death");
        public static readonly int Rotate_Platform_Bool = Animator.StringToHash("Rotate");
        public static readonly int Main_Menu_Rotate_Bool = Animator.StringToHash("MainMenuRotate");
        public static readonly int Platform_Double_Speed_Float = Animator.StringToHash("DoubleSpeed");

        #endregion

        #region UI Animations

        public static readonly int Remove_Playfield_Trigger = Animator.StringToHash("RemovePlayfield");
        public static readonly int Open_Playfield_Menu_Trigger = Animator.StringToHash("OpenPlayfieldMenu");
        public static readonly int Prefab_Menu_Bool = Animator.StringToHash("PrefabMenuEnter");
        
        #endregion
    }
}
