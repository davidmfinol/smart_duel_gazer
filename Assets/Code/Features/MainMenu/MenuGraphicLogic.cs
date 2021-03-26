using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.MainMenu
{
    public class MenuGraphicLogic : MonoBehaviour
    {
        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";

        private List<GameObject> _models = new List<GameObject>();
        private GameObject _currentModel;

        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager,
                              IScreenService screenService)
        {
            _dataManager = dataManager;

            screenService.UsePortraitOrientation();
            LoadModels();
        }

        private void LoadModels()
        {
            var models = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);

            foreach (GameObject obj in models)
            {
                _dataManager.GetCardModel(obj.name);
                var model = Instantiate(obj, transform);

                _models.Add(model);
                _dataManager.RecycleModel(model.name, model);
            }

            Resources.UnloadUnusedAssets();
        }

        public void ChooseRandomModelFromList()
        {
            int randomNum = Random.Range(0, _models.Count-1);
            if (_currentModel == null)
            {                               
                _currentModel = _dataManager.GetExistingModel(_models[randomNum].name);
                return;
            }

            _dataManager.RecycleModel(_currentModel.name, _currentModel);
            _currentModel = _dataManager.GetExistingModel(_models[randomNum].name);
        }
    }
}
