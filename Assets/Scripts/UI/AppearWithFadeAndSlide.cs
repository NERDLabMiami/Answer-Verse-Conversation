using System.Collections;
using UnityEngine;

public class AppearWithFadeAndSlide : MonoBehaviour
{
    public float slideDistance = 1f;
    public float duration = 0.5f;
    private SpriteRenderer sr;
    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f); // fully transparent

        endPos = transform.position;
        startPos = endPos + new Vector3(0, slideDistance, 0);
        transform.position = startPos;

        StartCoroutine(AnimateIn());
    }

    private IEnumerator AnimateIn()
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;

            // Lerp position
            transform.position = Vector3.Lerp(startPos, endPos, progress);

            // Fade in
            Color c = sr.color;
            c.a = Mathf.Clamp01(progress);
            sr.color = c;

            yield return null;
        }

        // Snap to final values
        transform.position = endPos;
        Color final = sr.color;
        final.a = 1f;
        sr.color = final;
    }
}