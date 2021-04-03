using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;
using UniRx;
using static AssemblyCSharp.Assets.Code.Features.ModelViewer.SummonModelButton;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Statics;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif

namespace AssemblyCSharp.Assets.Code.Features.ModelViewer
{
    public class ModelViewScrollBarLogic : MonoBehaviour
    {
        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";
        private const string RESOURCES_THUMBNAIL_FOLDER_NAME = "Thumbnails";

        [SerializeField] private GameObject _prefabManager;
        [SerializeField] private GameObject _contentMenu;
        [SerializeField] private GameObject _buttonPrefab;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _platform;

        private GameObject _model;

        private IDataManager _dataManager;
        private SummonModelButtonFactory _factory;
        
        [Inject]
        public void Construct(IDataManager dataManager,
                              IScreenService screenService,
                              SummonModelButtonFactory factory)
        {
            _dataManager = dataManager;
            _factory = factory;

            screenService.UseAutoLandscapeOrientation();
            LoadModels();
        }

        private void LoadModels()
        {
            var models = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);
            var thumbnails = Resources.LoadAll<Sprite>(RESOURCES_THUMBNAIL_FOLDER_NAME);

            foreach (GameObject obj in models)
            {
                _dataManager.GetCardModel(obj.name);
                var model = Instantiate(obj, _prefabManager.transform);
                var button = _factory.Create(_buttonPrefab);

                button.GetComponent<Image>().sprite = thumbnails.SingleOrDefault(image => image.name == obj.name);
                button.GetComponentInChildren<TMP_Text>().text = model.transform.GetChild(0).name;
                button.GetComponent<Button>().onClick.AsObservable().Subscribe(_ => SummonMonster(button.name));
                button.transform.SetParent(_contentMenu.transform, false);
                button.name = model.name;

                _dataManager.RecycleModel(model.name, model);

#if UNITY_EDITOR
                //StartCoroutine(MakeThumbnailsForNewModels(obj));
#endif
            }

            Resources.UnloadUnusedAssets();
        }

        private void SummonMonster(string name)
        {
            if (_model != null)
            {
                _dataManager.RecycleModel(_model.name, _model);
            }

            _model = _dataManager.GetExistingModel(name);
            _model.transform.SetParent(_platform.transform);
            _model.transform.SetPositionAndRotation(_platform.position, _platform.rotation);

            _model.GetComponentInChildren<Animator>().SetTrigger(AnimatorParams.Summoning_Trigger);
        }

        public void PrefabMenuView(bool state)
        {
            _animator.SetBool(AnimatorParams.Prefab_Menu_Bool, state);
        }

#if UNITY_EDITOR
        private IEnumerator MakeThumbnailsForNewModels(GameObject model)
        {
            yield return AssetPreview.GetAssetPreview(model);
            var texture = AssetPreview.GetAssetPreview(model);
            File.WriteAllBytes(Application.streamingAssetsPath + '/' + model.name, texture.EncodeToJPG());
        }
#endif
    
    }
}
