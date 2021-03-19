using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModelViewerMovementLogic : MonoBehaviour
{
    [SerializeField]
    private Camera _mainCamera;

    [SerializeField]
    private Button _buttonY;

    private float currentY = 0f;
    //private float currentX = 0f;
    //private float currentZ = 0f;

    void Start()
    {
        
    }

    void Update()
    {

    }

    public void SwitchCameraView(bool state)
    {
        if(state)
        {
            _mainCamera.transform.position = new Vector3(0f, 1f, 3f);
            _mainCamera.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
        else if (!state)
        {
            _mainCamera.transform.position = new Vector3(-0.75f, 2f, 3f);
            _mainCamera.transform.rotation = Quaternion.Euler(16f, 168f, -3.3f);
        }
    }
    
    public void MoveYAxis()
    {
        _mainCamera.transform.position = new Vector3(0, currentY + 1, 0);
        currentY++;
        print(currentY);
    }

    public void MoveXAxis()
    {

    }

    public void MoveZAxis()
    {

    }
}
