using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.General
{
    public static class AnimatorIDSetter
    {
        #region Monster Animations
            
        public static readonly int Animator_Summoning_Trigger = Animator.StringToHash("SummoningTrigger");
        
        #endregion

        #region UI Animations
            
        public static readonly int Animator_Remove_Playfield = Animator.StringToHash("RemovePlayfield");
        public static readonly int Animator_Open_Playfield_Menu = Animator.StringToHash("OpenPlayfieldMenu");
        
        #endregion
    }
}
