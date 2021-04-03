using AssemblyCSharp.Assets.Code.Core.General.Statics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformLogic : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;

    private void Awake()
    {
        _animator.SetBool(AnimatorParams.Main_Menu_Rotate_Bool, false);
    }

    public void RotatePlatform(bool state)
    {
        _animator.SetBool(AnimatorParams.Rotate_Platform_Bool, state);
    }

    public void DoubleSpeed(bool state)
    {
        if(state)
        {
            _animator.SetFloat(AnimatorParams.Platform_Double_Speed_Float, 2f);
            return;
        }

        _animator.SetFloat(AnimatorParams.Platform_Double_Speed_Float, 1f);
    }
}
