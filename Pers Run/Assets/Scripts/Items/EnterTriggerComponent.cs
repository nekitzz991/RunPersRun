using System;
using UnityEngine;
using UnityEngine.Events;

namespace Pers.Components
{
    public class EnterTriggerComponent : MonoBehaviour
    {
        [SerializeField] private string _tag;
        [SerializeField] private LayerMask _layer;
        [SerializeField] private UnityEvent _onEnterTrigger; 

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & _layer) == 0) return;

            if (!string.IsNullOrEmpty(_tag) && !other.gameObject.CompareTag(_tag)) return;

            _onEnterTrigger?.Invoke();
        }
    }
}
