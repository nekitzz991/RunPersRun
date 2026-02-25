using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimationManager : MonoBehaviour
{
    [System.Serializable]
    public class AnimatedUIElement
    {
        public Image imageComponent; // Ссылка на UI Image
        public Sprite[] frames;      // Кадры анимации
        public float frameRate = 0.2f; // Скорость анимации
    }

    public AnimatedUIElement[] elements; // Массив анимируемых элементов
    private readonly System.Collections.Generic.List<Coroutine> runningCoroutines = new();

    private void OnEnable()
    {
        StartAnimations();
    }

    private void OnDisable()
    {
        StopAnimations();
    }

    private void StartAnimations()
    {
        StopAnimations();

        if (elements == null)
        {
            return;
        }

        foreach (var element in elements)
        {
            if (element == null || element.imageComponent == null || element.frames == null || element.frames.Length == 0)
            {
                continue;
            }

            runningCoroutines.Add(StartCoroutine(AnimateElement(element)));
        }
    }

    private void StopAnimations()
    {
        for (int i = 0; i < runningCoroutines.Count; i++)
        {
            if (runningCoroutines[i] != null)
            {
                StopCoroutine(runningCoroutines[i]);
            }
        }
        runningCoroutines.Clear();
    }

    private IEnumerator AnimateElement(AnimatedUIElement element)
    {
        int frameIndex = 0;
        float frameDelay = Mathf.Max(0.01f, element.frameRate);

        while (true)
        {
            element.imageComponent.sprite = element.frames[frameIndex];
            frameIndex = (frameIndex + 1) % element.frames.Length;
            yield return new WaitForSecondsRealtime(frameDelay);
        }
    }
}
