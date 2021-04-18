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
        private const string API_IMAGE_URL = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";

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

        public void SetCardImage(EventNames eventName, string zone, string cardID, bool isMonster)
        {
            StartCoroutine(AwaitAndSetImage(eventName, zone, cardID, isMonster));
        }

        #region Coroutines

        private IEnumerator AwaitAndSetImage(EventNames eventName, string zone, string cardID, bool isMonster)
        {
            yield return TextureWebRequest(cardID);

            _modelEventHandler.RaiseSummonSetCardEvent(zone, cardID, isMonster);

            switch (eventName)
            {
                case EventNames.SpellTrapActivate:
                    _modelEventHandler.RaiseEventByEventName(EventNames.SpellTrapActivate, zone);
                    break;
                case EventNames.RevealSetMonster:
                    _modelEventHandler.RaiseEventByEventName(EventNames.RevealSetMonster, zone);
                    break;
                default:
                    break;
            }
        }

        private IEnumerator TextureWebRequest(string cardID)
        {
            if (!_dataManager.DoesCachedImageExist(cardID))
            {
                string URL = string.Format(API_IMAGE_URL, cardID.RemoveStartingZeroIfRequired());

                using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(URL);
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Error {webRequest.error} on cardID: {cardID}");
                    yield return null;
                }

                DownloadHandlerTexture handlerTexture = webRequest.downloadHandler as DownloadHandlerTexture;

                _dataManager.CacheImage(cardID, handlerTexture.texture);
            }

            yield return null;
        }

        #endregion

        public class Factory : PlaceholderFactory<TextureRequest>
        {
        }

    }
}