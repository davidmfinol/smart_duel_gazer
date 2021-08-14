using Code.Core.Navigation;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI_Components.General
{
    public class ShowConnectionScene : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        private INavigationService _navigationService;

        #region Constructors

        [Inject]
        public void Construct(
            INavigationService navigationService)
        {
            _navigationService = navigationService;

            RegisterClickListeners();
        }

        #endregion

        private void RegisterClickListeners()
        {
            _button.onClick.AsObservable().Subscribe(_ => OnButtonPressed());
        }

        private void OnButtonPressed()
        {
            _navigationService.ShowConnectionScene();
        }
    }
}
