using AssemblyCSharp.Assets.Code.Core.General.Statics;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class TapForAttackAnimations : MonoBehaviour
    {
        private Camera _camera;

        void Awake()
        {
            _camera = Camera.main;
        }

        void Update()
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                SendRaycast(Input.GetTouch(0));
            }
        }

        private void SendRaycast(Touch touch)
        {
            var ray = _camera.ScreenPointToRay(touch.position);
            var hitSomething = Physics.Raycast(ray, out RaycastHit hit);

            if (!hitSomething)
            {
                return;
            }

            PlayAttackAnimation(hit);
        }

        private void PlayAttackAnimation(RaycastHit hit)
        {
            var model = hit.transform;

            var animator = model.GetComponent<Animator>();
            if (animator == null)
            {
                return;
            }

            animator.SetTrigger(AnimatorParams.Play_Monster_Attack_1_Trigger);
        }
    }
}
