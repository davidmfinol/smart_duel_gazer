using Code.Features.SpeedDuel.PrefabManager.Prefabs.Playfield.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Features.SpeedDuel
{
    //TODO: Make this class redundant and move logic to SpeedDuelViewModel
    public class PlayfieldMenuComponentsManager : MonoBehaviour
    {
        private GameObject _playfield;
        private Slider _rotationSlider;
        private Slider _scaleSlider;

        public void InitMenus(
            Slider rotationSlider,
            Slider scaleSlider)
        {
            _rotationSlider = rotationSlider;
            _scaleSlider = scaleSlider;
        }

        public PlayfieldTransformValues SetSliderValues()
        {
            _playfield = FindObjectOfType<PlayfieldComponentsManager>().gameObject;
            if (_playfield == null) return new PlayfieldTransformValues();

            var scale = _playfield.transform.localScale.x;
            var rotation = _playfield.transform.localRotation.y;

            if (scale > 10f)
            {
                _scaleSlider.maxValue = scale;
            }

            return new PlayfieldTransformValues { Scale = scale, Rotation = rotation };
        }
    }

    public class PlayfieldTransformValues
    {
        public float Scale;
        public float Rotation;
    }
}