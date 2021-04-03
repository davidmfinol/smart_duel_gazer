using UnityEngine;
using UniRx;
using UnityEngine.UI;
using AssemblyCSharp.Assets.Code.Core.Navigation.Interface;
using Zenject;
using System.Collections.Generic;
using System.Collections;

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
            StartCoroutine(AnimationsBuffer(1));
        }

        private void ShowSpeedDuelScene()
        {
            StartCoroutine(AnimationsBuffer(2));
        }

        private IEnumerator AnimationsBuffer(int i)
        {
            yield return new WaitForSeconds(2);

            switch(i)
            {
                case 1:
                    _navigation.ShowModelViewScene();
                    break;
                case 2:
                    _navigation.ShowSpeedDuelScene();
                    break;
            }
        }
    }
}
