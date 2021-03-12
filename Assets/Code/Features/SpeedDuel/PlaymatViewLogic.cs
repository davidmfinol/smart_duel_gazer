using UnityEngine;
using UnityEngine.UI;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.General;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class PlaymatViewLogic : MonoBehaviour
    {
        private static readonly string Indicator_Tag = "Indicator";

        [SerializeField]
        private GameObject _playmatShell;
        [SerializeField]
        private GameObject _menu;
        [SerializeField]
        private Slider _scaleSlider;

        private MeshRenderer[] _renderers;
        private Animator[] _animators;
        private GameObject _interaction;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _animators = GetComponentsInChildren<Animator>();
            //TODO Create a more solid connection to Indicator object
            _interaction = GameObject.FindGameObjectWithTag(Indicator_Tag);
        }

        public void SetScaleStartPosition(float startPosition) => _scaleSlider.value = startPosition;

        public void ScalePlaymat(float scale) 
        {
            var mappedScale = scale.Map(_scaleSlider.minValue, _scaleSlider.maxValue, 0.1f, 5f);
            _playmatShell.transform.localScale = new Vector3(mappedScale, mappedScale, mappedScale);
        }

        public void RotatePlaymat(float rotation) => _playmatShell.transform.rotation = Quaternion.Euler(0, rotation, 0);

        public void ChangePlaymatHeight(float height) => _playmatShell.transform.position = new Vector3(
            _playmatShell.transform.position.x, 
            height, 
            _playmatShell.transform.position.z
            );

        public void ShowSettingsMenu(bool state)
        {
            foreach (Animator animator in _animators)
            {
                animator.SetBool(AnimatorIDSetter.Animator_Open_Playfield_Menu, state);
            }
        }

        public void HidePlaymat(bool state)
        {
            foreach (MeshRenderer item in _renderers)
            {
                item.enabled = state;
            }
        }

        public void DeletePlaymat()
        {
            foreach (Animator animator in _animators)
            {
                animator.SetTrigger(AnimatorIDSetter.Animator_Remove_Playfield);
            }
        }

        private void DestroyPlaymat()
        {
            _interaction.BroadcastMessage("OnPlaymatDestroyed");
            Destroy(_playmatShell);
        }
    }
}
