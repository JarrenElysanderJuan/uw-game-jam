using UnityEngine;

public class TargetManager : MonoBehaviour
{
    void Start()
    {
        Blob[] blobs = FindObjectsByType<Blob>(FindObjectsSortMode.None);
        int rand = Random.Range(0, blobs.Length);
        blobs[rand].isTarget = true;
        blobs[rand].GetComponent<Renderer>().material.color = Color.red;
    }
}