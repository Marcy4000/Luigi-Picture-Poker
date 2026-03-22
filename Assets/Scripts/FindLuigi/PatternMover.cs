using UnityEngine;

public class PatternMover : MonoBehaviour
{
    private RectTransform rt;
    private float speed;
    private bool moveHorizontally;
    private float wrapThreshold;
    private Rect gameBounds;

    public void Initialize(Rect gameBounds, bool horizontal, float moveSpeed)
    {
        rt = GetComponent<RectTransform>();
        moveHorizontally = horizontal;
        speed = moveSpeed;
        this.gameBounds = gameBounds;
        wrapThreshold = moveHorizontally ? gameBounds.width : gameBounds.height;
    }

    void Update()
    {
        if (rt == null) return;

        Vector2 pos = rt.anchoredPosition;

        if (moveHorizontally)
        {
            pos.x += speed * Time.deltaTime;
            // Keep parent position roughly in reasonable range to avoid large numbers
            if (speed > 0 && pos.x > wrapThreshold)
                pos.x -= wrapThreshold;
            else if (speed < 0 && pos.x < -wrapThreshold)
                pos.x += wrapThreshold;
        }
        else
        {
            pos.y += speed * Time.deltaTime;
            if (speed > 0 && pos.y > wrapThreshold)
                pos.y -= wrapThreshold;
            else if (speed < 0 && pos.y < -wrapThreshold)
                pos.y += wrapThreshold;
        }

        rt.anchoredPosition = pos;

        // Ensure child elements wrap individually when they go fully off the visible game bounds.
        // Child effective center in game area = child.anchoredPosition + parent.anchoredPosition
        RectTransform[] children = rt.GetComponentsInChildren<RectTransform>(false);
        foreach (RectTransform child in children)
        {
            if (child == rt) continue; // skip self

            Vector2 childAnch = child.anchoredPosition;

            if (moveHorizontally)
            {
                float halfWidth = (child.rect.width * child.lossyScale.x) * 0.5f;
                float effectiveCenterX = childAnch.x + rt.anchoredPosition.x;

                float leftLimit = gameBounds.xMin;
                float rightLimit = gameBounds.xMax;

                // Span to move between 'just outside left' and 'just outside right'
                float span = gameBounds.width + 2f * halfWidth;

                // If completely past the right edge, move to just past left edge preserving overshoot
                if (effectiveCenterX - halfWidth > rightLimit)
                {
                    float overshoot = effectiveCenterX - (rightLimit + halfWidth);
                    int spans = Mathf.CeilToInt(overshoot / span);
                    float newCenter = (leftLimit - halfWidth) + (overshoot - (spans - 1) * span);
                    // If calculation overshoots, fallback to single span move
                    if (float.IsNaN(newCenter)) newCenter = leftLimit - halfWidth;
                    childAnch.x = newCenter - rt.anchoredPosition.x;
                }
                // If completely past the left edge, move to just past right edge preserving overshoot
                else if (effectiveCenterX + halfWidth < leftLimit)
                {
                    float overshoot = (leftLimit - halfWidth) - effectiveCenterX;
                    int spans = Mathf.CeilToInt(overshoot / span);
                    float newCenter = (rightLimit + halfWidth) - (overshoot - (spans - 1) * span);
                    if (float.IsNaN(newCenter)) newCenter = rightLimit + halfWidth;
                    childAnch.x = newCenter - rt.anchoredPosition.x;
                }
            }
            else
            {
                float halfHeight = (child.rect.height * child.lossyScale.y) * 0.5f;
                float effectiveCenterY = childAnch.y + rt.anchoredPosition.y;

                float bottomLimit = gameBounds.yMin;
                float topLimit = gameBounds.yMax;

                float span = gameBounds.height + 2f * halfHeight;

                if (effectiveCenterY - halfHeight > topLimit)
                {
                    float overshoot = effectiveCenterY - (topLimit + halfHeight);
                    int spans = Mathf.CeilToInt(overshoot / span);
                    float newCenter = (bottomLimit - halfHeight) + (overshoot - (spans - 1) * span);
                    if (float.IsNaN(newCenter)) newCenter = bottomLimit - halfHeight;
                    childAnch.y = newCenter - rt.anchoredPosition.y;
                }
                else if (effectiveCenterY + halfHeight < bottomLimit)
                {
                    float overshoot = (bottomLimit - halfHeight) - effectiveCenterY;
                    int spans = Mathf.CeilToInt(overshoot / span);
                    float newCenter = (topLimit + halfHeight) - (overshoot - (spans - 1) * span);
                    if (float.IsNaN(newCenter)) newCenter = topLimit + halfHeight;
                    childAnch.y = newCenter - rt.anchoredPosition.y;
                }
            }

            child.anchoredPosition = childAnch;
        }
    }
}