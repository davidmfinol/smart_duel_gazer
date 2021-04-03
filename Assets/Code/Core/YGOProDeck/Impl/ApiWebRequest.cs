using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using AssemblyCSharp.Assets.Code.Core.General.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl
{
    public class ApiWebRequest : MonoBehaviour
    {        
        private const string API_URL = "https://db.ygoprodeck.com/api/v7/cardinfo.php?id={0}";
        private const string API_IMAGE_URL = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";
        
        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public IEnumerator GetRequest(string cardID)
        {
            if (!_dataManager.CheckForCachedImage(cardID))
            {
                string URL = string.Format(API_IMAGE_URL, cardID.RemoveStartingZero());

                using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(URL);
                yield return webRequest.SendWebRequest();

                if(webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogWarning($"Error {webRequest.error} on cardID: {cardID}");
                    yield return null;
                }
                    
                DownloadHandlerTexture handlerTexture = webRequest.downloadHandler as DownloadHandlerTexture;

                _dataManager.CacheImage(cardID, handlerTexture.texture);
            }

            yield break;
        }
    }
}
