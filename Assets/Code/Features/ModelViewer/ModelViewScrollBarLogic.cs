using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Zenject;
using TMPro;
//using UnityEditor;
using UnityEngine.UI;
using static AssemblyCSharp.Assets.Code.Features.ModelViewer.SummonModelButton;
using AssemblyCSharp.Assets.Code.Core.Screen.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Statics;

namespace AssemblyCSharp.Assets.Code.Features.ModelViewer
{
    public class ModelViewScrollBarLogic : MonoBehaviour
    {
        private const string RESOURCES_MONSTERS_FOLDER_NAME = "Monsters";

        [SerializeField]
        private GameObject _prefabManager;
        [SerializeField]
        private GameObject _contentMenu;
        [SerializeField]
        private GameObject _buttonPrefab;

        private Animator _animator;

        private IDataManager _dataManager;
        private SummonModelButtonFactory _factory;
        
        [Inject]
        public void Construct(IDataManager dataManager,
                              IScreenService screenService,
                              SummonModelButtonFactory factory)
        {
            _dataManager = dataManager;
            _factory = factory;

            screenService.UseLandscapeOrientation();
            LoadModels();
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        private void LoadModels()
        {
            var models = Resources.LoadAll<GameObject>(RESOURCES_MONSTERS_FOLDER_NAME);

            foreach (GameObject obj in models)
            {
                _dataManager.GetCardModel(obj.name);
                var model = Instantiate(obj, _prefabManager.transform);
                var button = _factory.Create(_buttonPrefab);
                button.transform.SetParent(_contentMenu.transform, false);

                //StartCoroutine(SetButtonInfo(button, obj));
                button.GetComponentInChildren<TMP_Text>().text = model.transform.GetChild(0).name;
                button.name = obj.name;

                _dataManager.RecycleModel(obj.name, model);
            }

            Resources.UnloadUnusedAssets();
        }

        public void PrefabMenuView(bool state)
        {
            _animator.SetBool(AnimatorParams.Prefab_Menu_Bool, state);
        }

        //private IEnumerator SetButtonInfo(SummonModelButton button, GameObject model)
        //{
        //    yield return StartCoroutine(AwaitAssetPreviews(model));
        //    var texture = AssetPreview.GetAssetPreview(model);
        //    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        //    button.GetComponent<Image>().sprite = sprite;

        //    button.GetComponentInChildren<TMP_Text>().text = model.transform.GetChild(0).name;
        //    button.name = model.name;
        //}

        //private IEnumerator AwaitAssetPreviews(GameObject model) 
        //{
        //    yield return AssetPreview.GetAssetPreview(model);
        //}
    }
}
