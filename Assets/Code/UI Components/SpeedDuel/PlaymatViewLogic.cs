using UnityEngine;
using UnityEngine.UI;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.General.Statics;

namespace AssemblyCSharp.Assets.Code.UIComponents.SpeedDuel
{
    public class PlaymatViewLogic : MonoBehaviour
    {
        [SerializeField]
        private GameObject _playmatShell;
        [SerializeField]
        private GameObject _bottomMenu;
        [SerializeField]
        private Slider _scaleSlider;
        [SerializeField]
        private Toggle _menuToggle;

        private MeshRenderer[] _renderers;
        private Animator[] _animators;
        private GameObject _interaction;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>();
            _animators = GetComponentsInChildren<Animator>();
            _interaction = GameObject.FindGameObjectWithTag(Tags.Indicator);
        }

        public void SetScaleStartPosition(float startPosition) => _scaleSlider.value = startPosition;

        public void ScalePlaymat(float scale) 
        {
            var mappedScale = MapScaleValueToSliderValue(scale, _scaleSlider.minValue, _scaleSlider.maxValue, 0.1f, 5f);
            _playmatShell.transform.localScale = new Vector3(mappedScale, mappedScale, mappedScale);
        }

        private float MapScaleValueToSliderValue(float value,
                                                 float originalMin,
                                                 float originalMax,
                                                 float newMin,
                                                 float newMax)
        {
            return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
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
                animator.SetBool(AnimatorParams.Open_Playfield_Menu_Trigger, state);
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
                animator.SetTrigger(AnimatorParams.Remove_Playfield_Trigger);
            }
            _menuToggle.isOn = false;
        }

        private void DestroyPlaymat()
        {
            _interaction.BroadcastMessage("OnPlaymatDestroyed");
        }
    }
}
