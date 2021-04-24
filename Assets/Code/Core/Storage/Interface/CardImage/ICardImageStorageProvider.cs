using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.Storage.Interface.CardImage
{
    public interface ICardImageStorageProvider
    {
        public Texture GetCardImage(string cardId);
        public void SaveCardImage(string cardId, Texture image);
    }
}
