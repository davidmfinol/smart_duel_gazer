using System.Threading.Tasks;
using Code.Core.Config.Providers;
using Code.Core.Logger;
using UnityEngine;
using UnityEngine.Networking;
using Zenject;

namespace Code.Core.YGOProDeck
{
    public interface IYgoProDeckApiProvider
    {
        public Task<Texture> GetCardImage(string cardId);
    }
    
    public class YgoProDeckApiProvider : IYgoProDeckApiProvider
    {
        private const string Tag = "YgoProDeckApiProvider";
        private const string ImageBaseUrl = "https://storage.googleapis.com/ygoprodeck.com/pics/{0}.jpg";
        
        // One second divided by 60 Hertz.
        private const int AsyncOperationStatusCheckDelayInMs = 1000 / 60;

        private readonly IDelayProvider _delayProvider;
        private readonly IAppLogger _logger;

        [Inject]
        public YgoProDeckApiProvider(
            IDelayProvider delayProvider,
            IAppLogger logger)
        {
            _delayProvider = delayProvider;
            _logger = logger;
        }

        public async Task<Texture> GetCardImage(string cardId)
        {
            var url = string.Format(ImageBaseUrl, cardId);
            using var request = UnityWebRequestTexture.GetTexture(url);

            var asyncOperation = request.SendWebRequest();
            while (!asyncOperation.isDone)
            {
                await _delayProvider.Wait(AsyncOperationStatusCheckDelayInMs);
            }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                _logger.Warning(Tag, $"An error occurred while fetching the card of {cardId}: {request.result}");
                return null;
            }

            return DownloadHandlerTexture.GetContent(request);
        }
    }
}
