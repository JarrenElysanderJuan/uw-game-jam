using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        instance = this;
    }

    public void Win()
    {
        Debug.Log("YOU WIN");
        Time.timeScale = 0f;
    }
}