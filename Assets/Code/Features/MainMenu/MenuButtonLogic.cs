using UnityEngine;
using UniRx;
using UnityEngine.UI;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.MainMenu
{
    public class MenuButtonLogic : MonoBehaviour
    {
        [SerializeField]
        private Button _speedDuelButton;
        [SerializeField]
        private Button _modelViewButton;

        private INavigationService _navigation;

        [Inject]
        public void Construct(INavigationService navigation)
        {
            _navigation = navigation;
        }

        void Awake()
        {
            _speedDuelButton.onClick.AsObservable().Subscribe(_ => ShowSpeedDuelScene());
            _modelViewButton.onClick.AsObservable().Subscribe(_ => ShowModelViewScene());
        }

        private void ShowModelViewScene()
        {
            _navigation.ShowModelViewScene();
        }

        private void ShowSpeedDuelScene()
        {
            _navigation.ShowSpeedDuelScene();
        }
    }
}
