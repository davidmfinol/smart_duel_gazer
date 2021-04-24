using Zenject;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.Models.Impl.ModelEventsHandler;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl.ApiTextureRequest
{
    public class TextureRequest : MonoBehaviour
    {
        private const string ImageBaseUrl = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";

        private IDataManager _dataManager;
        private ModelEventHandler _modelEventHandler;

        #region Constructor

        [Inject]
        public void Construct(IDataManager dataManager,
                              ModelEventHandler modelEventHandler)
        {
            _dataManager = dataManager;
            _modelEventHandler = modelEventHandler;
        }

        #endregion

        public void SetCardImage(ModelEvent modelEvent, string zone, string cardId, bool isMonster)
        {
            StartCoroutine(AwaitAndSetImage(modelEvent, zone, cardId, isMonster));
        }

        #region Coroutines

        private IEnumerator AwaitAndSetImage(ModelEvent modelEvent, string zone, string cardId, bool isMonster)
        {
            yield return TextureWebRequest(cardId);

            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardId, isMonster);

            switch (modelEvent)
            {
                case ModelEvent.SpellTrapActivate:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.SpellTrapActivate, zone);
                    break;
                case ModelEvent.RevealSetMonster:
                    _modelEventHandler.RaiseEventByEventName(ModelEvent.RevealSetMonster, zone);
                    break;
            }
        }

        private IEnumerator TextureWebRequest(string cardId)
        {
            if (!_dataManager.IsImageRecyclable(cardId))
            {
                string URL = string.Format(ImageBaseUrl, cardId.RemoveLeadingZero());

                using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(URL);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Error {webRequest.error} on cardId: {cardId}");
                    yield return null;
                }

                DownloadHandlerTexture handlerTexture = webRequest.downloadHandler as DownloadHandlerTexture;

                _dataManager.SaveImage(cardId, handlerTexture.texture);
            }

            yield return null;
        }

        #endregion

        public class Factory : PlaceholderFactory<TextureRequest>
        {
        }

    }
}