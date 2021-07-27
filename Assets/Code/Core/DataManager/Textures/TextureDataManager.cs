using System.Threading.Tasks;
using Code.Core.Storage.Texture;
using Code.Core.YGOProDeck;
using UnityEngine;
using Zenject;

namespace Code.Core.DataManager.Textures
{
    public interface ITextureDataManager
    {
        Task<Texture> GetCardImage(string cardId);
    }
    
    public class TextureDataManager : ITextureDataManager
    {
        private readonly IYgoProDeckApiProvider _ygoProDeckApiProvider;
        private readonly ITextureStorageProvider _textureStorageProvider;

        [Inject]
        public TextureDataManager(
            IYgoProDeckApiProvider ygoProDeckApiProvider,
            ITextureStorageProvider textureStorageProvider)
        {
            _ygoProDeckApiProvider = ygoProDeckApiProvider;
            _textureStorageProvider = textureStorageProvider;
        }

        public async Task<Texture> GetCardImage(string cardId)
        {
            Debug.Log($"GetCardImage(cardId: {cardId})");

            var image = _textureStorageProvider.GetTexture(cardId);
            if (image != null)
            {
                return image;
            }

            image = await _ygoProDeckApiProvider.GetCardImage(cardId);
            _textureStorageProvider.SaveTexture(cardId, image);
            return image;
        }
    }
}