using UnityEngine;

public class DestroyObjectComponent : MonoBehaviour
{
    [SerializeField] private GameObject _objectToDestroy;

    public void DestroyObject()
    {
        if (_objectToDestroy != null)
        {
            Destroy(_objectToDestroy);
            return;
        }

        Destroy(gameObject);
    }
}
