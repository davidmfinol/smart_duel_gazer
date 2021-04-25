using System.Collections.Generic;
using AssemblyCSharp.Assets.Code.Core.Storage.Interface.CardImage;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Impl.CardImage
{
    public class CardImageStorageProvider : ICardImageStorageProvider
    {
        private readonly Dictionary<string, Texture> _images = new Dictionary<string, Texture>();

        public Texture GetCardImage(string cardId)
        {
            var hasImage = _images.TryGetValue(cardId, out var image);
            return hasImage ? image : null;
        }

        public void SaveCardImage(string cardId, Texture image)
        {
            _images[cardId] = image;
        }
    }
}
