using System.Threading.Tasks;
using Code.Core.Logger;
using Code.Core.Storage.Texture;
using Code.Core.YGOProDeck;
using UnityEngine;

namespace Code.Core.DataManager.Textures
{
    public interface ITextureDataManager
    {
        Task<Texture> GetCardImage(string cardId);
    }

    public class TextureDataManager : ITextureDataManager
    {
        private const string Tag = "TextureDataManager";

        private readonly IYgoProDeckApiProvider _ygoProDeckApiProvider;
        private readonly ITextureStorageProvider _textureStorageProvider;
        private readonly IAppLogger _logger;

        public TextureDataManager(
            IYgoProDeckApiProvider ygoProDeckApiProvider,
            ITextureStorageProvider textureStorageProvider,
            IAppLogger logger)
        {
            _ygoProDeckApiProvider = ygoProDeckApiProvider;
            _textureStorageProvider = textureStorageProvider;
            _logger = logger;
        }

        public async Task<Texture> GetCardImage(string cardId)
        {
            _logger.Log(Tag, $"GetCardImage(cardId: {cardId})");

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