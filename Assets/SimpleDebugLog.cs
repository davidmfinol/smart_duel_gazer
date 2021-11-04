using UnityEngine;

public class SimpleDebugLog : MonoBehaviour
{
    public void LogSomethingHappened()
    {
        Debug.LogWarning("Something Happened!!");
    }
}
