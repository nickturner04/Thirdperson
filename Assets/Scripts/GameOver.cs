using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [SerializeField] private Image image;
    private void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        for (float i = 0; i <= 1; i+=0.01f)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, i);
            yield return new WaitForSeconds(0.01f);
        }
        yield return null;
    }
}
