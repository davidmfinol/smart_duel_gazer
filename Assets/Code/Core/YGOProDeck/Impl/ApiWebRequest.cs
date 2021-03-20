using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl
{
    public class ApiWebRequest : MonoBehaviour
    {
        private const string API_URL = "https://db.ygoprodeck.com/api/v7/cardinfo.php?id=";
        private const string API_IMAGE_URL = "https://storage.googleapis.com/ygoprodeck.com/pics/";
        private const string IMAGE_FILE_EXT = ".jpg";

        private IDataManager _dataManager;

        [Inject]
        public void Construct(IDataManager dataManager)
        {
            _dataManager = dataManager;
        }

        public void GetImageFromAPI(string cardID)
        {
            if (!_dataManager.CheckForCachedImage(cardID))
            {
                string url = API_IMAGE_URL + cardID + IMAGE_FILE_EXT;
                StartCoroutine(GetRequest(url, cardID));
            }
        }

        private IEnumerator GetRequest(string URL, string cardID)
        {           
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(URL);
            yield return webRequest.SendWebRequest();
            DownloadHandlerTexture handlerTexture = webRequest.downloadHandler as DownloadHandlerTexture;

            _dataManager.CacheImage(cardID, handlerTexture.texture);
        }
    }
}
