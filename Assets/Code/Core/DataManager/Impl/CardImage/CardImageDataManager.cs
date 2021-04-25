using System.Threading.Tasks;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardImage;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.CardImage;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using UnityEngine;
using Zenject;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Impl.CardImage
{
    public class CardImageDataManager : ICardImageDataManager
    {
        private readonly IYGOProDeckApiProvider _ygoProDeckApiProvider;
        private readonly ICardImageStorageProvider _cardImageStorageProvider;

        [Inject]
        public CardImageDataManager(
            ICardImageStorageProvider cardImageStorageProvider,
            IYGOProDeckApiProvider ygoProDeckApiProvider)
        {
            _cardImageStorageProvider = cardImageStorageProvider;
            _ygoProDeckApiProvider = ygoProDeckApiProvider;
        }

        public async Task<Texture> GetCardImage(string cardId)
        {
            Debug.Log($"GetCardImage(cardId: {cardId})");

            var image = _cardImageStorageProvider.GetCardImage(cardId);
            if (image != null)
            {
                return image;
            }

            image = await _ygoProDeckApiProvider.GetCardImage(cardId);
            _cardImageStorageProvider.SaveCardImage(cardId, image);
            return image;
        }
    }
}