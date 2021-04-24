using Zenject;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface;
using AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl.ApiTextureRequest;
using AssemblyCSharp.Assets.Code.Core.Models.Interface.ModelEventsHandler.Entities;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Impl
{
    public class ApiWebRequest : IApiWebRequest
    {
        private TextureRequest _textureRequest;
        private TextureRequest.Factory _factory;

        [Inject]
        public void Construct(TextureRequest.Factory factory)
        {
            _factory = factory;
        }        

        public void RequestCardImageFromWeb(ModelEvent modelEvent, string zone, string cardId, bool isMonster)
        {
            if (_textureRequest == null)
            {
                _textureRequest = _factory.Create();
            }
            
            _textureRequest.SetCardImage(modelEvent, zone, cardId, isMonster);
        }
    }
}
