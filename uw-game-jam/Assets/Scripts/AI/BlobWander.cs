using UnityEngine;

public class BlobWander : MonoBehaviour
{
    public float speed = 2f;
    Vector3 direction;
    float timer;

    void Start()
    {
        ChangeDirection();
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);

        timer -= Time.deltaTime;
        if (timer <= 0)
            ChangeDirection();
    }

    void ChangeDirection()
    {
        direction = new Vector3(Random.Range(-1f,1f),0,Random.Range(-1f,1f)).normalized;
        timer = Random.Range(1f, 3f);
    }
}