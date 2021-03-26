using System.Collections;
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

        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void ChangeImage(Texture texture) => _image.material.SetTexture("_MainTex", texture);

        //TODO: Find out why DataManager isn't initializing
        public void ChangeImageFromAPI(string cardID) => _image.material.SetTexture("_MainTex", _dataManager.GetCachedImage(cardID));
    }
}
