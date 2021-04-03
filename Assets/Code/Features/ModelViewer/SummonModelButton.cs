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
        }

        public class SummonModelButtonFactory : PlaceholderFactory<GameObject, SummonModelButton> 
        {
        }
    }
}
