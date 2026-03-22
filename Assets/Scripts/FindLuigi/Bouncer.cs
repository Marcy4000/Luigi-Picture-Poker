using UnityEngine;

public class Bouncer : MonoBehaviour
{
    private RectTransform rt;
    private Rect bounds;
    private Vector2 velocity;
    private float speed;

    public void Initialize(Rect gameBounds, float moveSpeed)
    {
        rt = GetComponent<RectTransform>();
        bounds = gameBounds;
        speed = moveSpeed;
        velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized * speed;
    }

    void Update()
    {
        if (rt == null) return;

        Vector2 pos = rt.anchoredPosition;
        pos += velocity * Time.deltaTime;

        // Bounce off walls
        // Assumes pivots are centered
        float halfWidth = rt.rect.width / 2;
        float halfHeight = rt.rect.height / 2;

        if (pos.x < bounds.xMin + halfWidth)
        {
            pos.x = bounds.xMin + halfWidth;
            velocity.x = -velocity.x;
        }
        else if (pos.x > bounds.xMax - halfWidth)
        {
            pos.x = bounds.xMax - halfWidth;
            velocity.x = -velocity.x;
        }

        if (pos.y < bounds.yMin + halfHeight)
        {
            pos.y = bounds.yMin + halfHeight;
            velocity.y = -velocity.y;
        }
        else if (pos.y > bounds.yMax - halfHeight)
        {
            pos.y = bounds.yMax - halfHeight;
            velocity.y = -velocity.y;
        }

        rt.anchoredPosition = pos;
    }
}