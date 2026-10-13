using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct TranslatedText
{
    public string text;
    public string translation;
}

public class Thought : MonoBehaviour
{
    public Image character;
    public TextMeshProUGUI thought;
    public TextMeshProUGUI subtitle;
    public float fadeDuration = 0.1f;
    
//    private Color transparentColor = new Color(1f, 1f, 1f, 0f);

    public void ResetColors()
    {
        Color thoughtColor = thought.color;
        Color subtitleColor = subtitle.color;
        thoughtColor.a = 1;
        subtitleColor.a = 1;
        thought.color = thoughtColor;
        subtitle.color = subtitleColor;
    }
    public void SetThought(TranslatedText t)
    {
        
        thought.text = t.text;

        if (t.translation.Length == 0)
        {
            subtitle.gameObject.SetActive(false);
        }
        else
        {
            subtitle.text = t.translation;            
            subtitle.gameObject.SetActive(true);
            StartCoroutine(FadeInSubtitle());
        }
        StartCoroutine(FadeInThought());

    }

    private IEnumerator FadeInThought()
    {
        Color fixedColor = thought.color;
        fixedColor.a = 1;
        thought.color = fixedColor;
        thought.CrossFadeAlpha(0f, 0f, true);
        thought.CrossFadeAlpha(1, fadeDuration,true);        
        yield return new WaitForSeconds(fadeDuration);
    }

    private IEnumerator FadeInSubtitle()
    {
        Color fixedColor = subtitle.color;
        fixedColor.a = 1;
        subtitle.color = fixedColor;
        subtitle.CrossFadeAlpha(0f, 0f, true);
        subtitle.CrossFadeAlpha(1f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }

    public void SetCharacter(Sprite sprite)
    {
        character.sprite = sprite;
    }

    public void FadeOutText()
    {
        Debug.Log("Fading Out: " + thought.text);
        StartCoroutine(FadeOutThought());
        StartCoroutine(FadeOutSubtitle());
    }

    private IEnumerator FadeOutThought()
    {
        Debug.Log("Calling Fade Out Thought: " + thought.color.a);

        thought.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }

    private IEnumerator FadeOutSubtitle()
    {
        subtitle.CrossFadeAlpha(0f, fadeDuration, false);
        yield return new WaitForSeconds(fadeDuration);
    }
}
