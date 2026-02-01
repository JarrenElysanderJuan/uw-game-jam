using UnityEngine;

public class Blob : MonoBehaviour
{
    public bool isTarget = false;

    public void Die()
    {
        if (isTarget)
        {
            //GameManager.instance.Win();
        }
        Destroy(gameObject);
    }
}