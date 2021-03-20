using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRandomImageFromList : MonoBehaviour, IImageSetter
{
    [SerializeField]
    private Renderer _image;
    [SerializeField]
    private List<Texture> _cardImages;

    public void ChangeImage(Texture texture)
    {
        var randomTexture = _cardImages[Random.Range(0, _cardImages.Count+1)];
        _image.material.SetTexture("_MainTex", randomTexture);
    }

    public void ChangeImageFromAPI(string cardID)
    {
        throw new System.NotImplementedException();
    }
}
