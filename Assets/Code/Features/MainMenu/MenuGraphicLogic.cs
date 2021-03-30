using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.MainMenu
{
    public class MenuGraphicLogic : MonoBehaviour
    {
        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";

        [SerializeField]
        private Button _speedDuelButton;
        [SerializeField]
        private Button _modelViewButton;
        [SerializeField]
        private GameObject _particles;
        
        private List<GameObject> _models = new List<GameObject>();
        private GameObject _currentModel;
        private int pastRandomNum;

        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager,
                              IScreenService screenService)
        {
            _dataManager = dataManager;

            screenService.UsePortraitOrientation();
            LoadModels();
        }

        private void Awake()
        {
            _speedDuelButton.onClick.AsObservable().Subscribe(_ => ModelViewerPressed());
            _modelViewButton.onClick.AsObservable().Subscribe(_ => SpeedDuelPressed());
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
            var randomNum = Random.Range(0, _models.Count-1);
            while(randomNum == pastRandomNum)
            {
                randomNum = Random.Range(0, _models.Count - 1);
            }
            
            if (_currentModel == null)
            {                               
                _currentModel = _dataManager.GetExistingModel(_models[randomNum].name);
                return;
            }

            _dataManager.RecycleModel(_currentModel.name, _currentModel);
            _currentModel = _dataManager.GetExistingModel(_models[randomNum].name);
            _currentModel.GetComponentInChildren<Animator>().SetTrigger(AnimatorParams.Summoning_Trigger);
            pastRandomNum = randomNum;
        }

        private void SpeedDuelPressed()
        {
            _currentModel.GetComponentInChildren<Animator>().SetTrigger(AnimatorParams.Play_Monster_Attack_1_Trigger);
        }

        private void ModelViewerPressed()
        {
            var particles = Instantiate(_particles, transform.position, transform.rotation, transform);
            particles.GetComponent<ISetMeshCharacter>().GetCharacterMesh(_currentModel.GetComponentInChildren<SkinnedMeshRenderer>());
            _dataManager.RecycleModel(_currentModel.name, _currentModel);
        }
    }
}
