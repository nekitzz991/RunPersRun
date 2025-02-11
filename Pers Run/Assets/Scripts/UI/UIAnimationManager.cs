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

    private void Start()
    {
        foreach (var element in elements)
        {
            StartCoroutine(AnimateElement(element));
        }
    }

   private IEnumerator AnimateElement(AnimatedUIElement element)
{
    int frameIndex = 0;
    while (true)
    {
        element.imageComponent.sprite = element.frames[frameIndex];
        frameIndex = (frameIndex + 1) % element.frames.Length;
        yield return new WaitForSecondsRealtime(element.frameRate); // Работает вне зависимости от Time.timeScale
    }
}

}
