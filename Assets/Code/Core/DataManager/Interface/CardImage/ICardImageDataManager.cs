using System.Threading.Tasks;
using UnityEngine;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.CardImage
{
    public interface ICardImageDataManager
    {
        Task<Texture> GetCardImage(string cardId);
    }
}