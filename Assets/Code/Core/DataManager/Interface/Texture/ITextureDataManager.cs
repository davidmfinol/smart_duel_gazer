using System.Threading.Tasks;

namespace AssemblyCSharp.Assets.Code.Core.DataManager.Interface.Texture
{
    public interface ITextureDataManager
    {
        Task<UnityEngine.Texture> GetCardImage(string cardId);
    }
}