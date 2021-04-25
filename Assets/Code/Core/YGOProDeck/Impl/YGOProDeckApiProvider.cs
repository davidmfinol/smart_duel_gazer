using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using UnityEngine;
using UnityEngine.Networking;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl
{
    public class YGOProDeckApiProvider : IYGOProDeckApiProvider
    {
        private const string ImageBaseUrl = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";

        public async Task<Texture> GetCardImage(string cardId)
        {
            var url = string.Format(ImageBaseUrl, cardId);
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                // TODO: create delay provider
                await Task.Delay(1000 / 30);
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log($"An error occurred while fetching the card of {cardId}: {request.result}");
                return null;
            }

            return DownloadHandlerTexture.GetContent(request);
        }
    }
}
