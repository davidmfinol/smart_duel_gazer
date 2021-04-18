using UnityEngine;

public interface IImageSetter
{
    void ChangeImageToTexture(Texture texture);
    void ChangeImageFromAPI(string cardID);
}
