using System.Threading.Tasks;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.YGOProDeck.Interface
{
    public interface IYGOProDeckApiProvider
    {
        public Task<Texture> GetCardImage(string cardId);
    }
}
