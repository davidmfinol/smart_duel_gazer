using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AssemblyCSharp.Assets.Code.Core.DataManager.Interface;
using Zenject;

public class SetImageFromAPI : MonoBehaviour, IImageSetter
{
    [SerializeField]
    private Renderer _image;
    public string cardID;
    
    private IDataManager _dataManager;

    [Inject]
    public void Construct (IDataManager dataManager)
    {
        _dataManager = dataManager;
    }

    
    public void ChangeImage()
    {
    }
    
    public void ChangeImageFromAPI(string cardID)
    {
        _image.material.SetTexture("_MainTex", _dataManager.GetCachedImage(cardID));
    }
}
