using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.ModelViewer
{
    public class SummonModelButton : MonoBehaviour
    {
        private GameObject _platform;
        private GameObject _model;
        private Button _button;

        private IDataManager _dataManager;        

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
            Init();
        }

        private void Init()
        {
            _platform = GameObject.FindGameObjectWithTag(Tags.Platform);
            _button = GetComponent<Button>();
            _button.onClick.AsObservable().Subscribe(_ => SummonModel());
        }

        private void SummonModel()
        {
            if (_model != null)
            {
                _dataManager.RecycleModel(_button.name, _model);
                _model = null;
                return;
            }
            
            _model = _dataManager.GetExistingModel(_button.name);
            _model.transform.SetParent(_platform.transform);
            _model.transform.SetPositionAndRotation(_platform.transform.position, _platform.transform.rotation);

            _model.GetComponentInChildren<Animator>().SetTrigger(AnimatorParams.Summoning_Trigger);
        }

        public class SummonModelButtonFactory : PlaceholderFactory<GameObject, SummonModelButton> 
        {
        }
    }
}
