using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Features.SpeedDuel
{
    public class SetImageFromAPI : MonoBehaviour, IImageSetter
    {
        [SerializeField]
        private Renderer _image;
        [SerializeField]
        private List<Texture> _errorImages = new List<Texture>();

        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void ChangeImageToTexture(Texture texture) => _image.material.SetTexture("_MainTex", texture);

        public void ChangeImageFromAPI(string cardID)
        {
            if (_dataManager.CheckForCachedImage(cardID))
            {
                _image.material.SetTexture("_MainTex", _dataManager.GetCachedImage(cardID));
                return;
            }
            SetRandomErrorImage();
        }

        private void SetRandomErrorImage()
        {
            var randomNum = Random.Range(0, _errorImages.Count);            
            _image.material.SetTexture("_MainTex", _errorImages[randomNum]);
        }
    }

    public class SetImageFromAPIFactory : PlaceholderFactory<GameObject, SetImageFromAPI>
    {
    }
}
