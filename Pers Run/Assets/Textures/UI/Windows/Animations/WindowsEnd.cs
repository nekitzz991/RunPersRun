using UnityEngine;

public class UIWindowAnimationHandler : MonoBehaviour
{
    // Этот метод будет вызван Animation Event в конце анимации Hide.
    public void OnHideAnimationEnd()
    {
        gameObject.SetActive(false);
    }
}
