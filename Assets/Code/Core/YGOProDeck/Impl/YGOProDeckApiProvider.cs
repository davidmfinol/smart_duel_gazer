using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.Config.Interface.Providers;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl
{
    public class YGOProDeckApiProvider : IYGOProDeckApiProvider
    {
        // One second divided by 60 Hertz.
        private const int AsyncOperationStatusCheckDelayInMs = 1000 / 60;
        private const string ImageBaseUrl = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";

        private readonly IDelayProvider _delayProvider;

        [Inject]
        public YGOProDeckApiProvider(
            IDelayProvider delayProvider)
        {
            _delayProvider = delayProvider;
        }

        public async Task<Texture> GetCardImage(string cardId)
        {
            var url = string.Format(ImageBaseUrl, cardId);
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);

            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await _delayProvider.Wait(AsyncOperationStatusCheckDelayInMs);
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
