using UnityEngine;

namespace Code.Core.Models
{
    public interface IImageSetter
    {
        void ChangeImageToTexture(Texture texture);
        void ChangeImageFromAPI(string cardID);
    }
}
