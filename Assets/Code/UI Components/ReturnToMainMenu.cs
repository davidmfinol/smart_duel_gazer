using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;

namespace AssemblyCSharp.Assets.Code.Core.General.Navigation
{
    public class ReturnToMainMenu : MonoBehaviour
    {
        [SerializeField]
        private Button _backButton;

        private INavigationService _navigationService;

        #region Constructors
        [Inject]
        public void Construct(INavigationService navigationService)
        {
            _navigationService = navigationService;

            InitNavigation();
        }
        #endregion

        private void InitNavigation()
        {
            _backButton.onClick.AsObservable().Subscribe(_ => OnBackButtonPressed());
        }

        public void OnBackButtonPressed()
        {
            _navigationService.ShowConnectionScene();
        }
    }
}
