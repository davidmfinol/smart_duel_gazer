using UnityEngine;

public interface IImageSetter
{
    void ChangeImage(Texture texture);
    void ChangeImageFromAPI(string cardID);
}
