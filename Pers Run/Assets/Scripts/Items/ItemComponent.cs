using UnityEngine;

public class ItemComponent : MonoBehaviour
{
    public enum ItemType { Bone, Poop }
    public enum BoneType { JustBone, GoldenBone }
    public enum PoopType { SmallPoop, BigPoop }
    
    [SerializeField] private ItemType itemType;
    [SerializeField] private BoneType boneVariant;
    [SerializeField] private PoopType poopVariant;
    
    [SerializeField] private AudioClip pickupSound;

    private int scoreValue;
    
    public ItemType Type => itemType;
    public int ScoreValue => scoreValue;
    
    private void Awake()
    {
        SetScoreValue();
    }
    
    private void SetScoreValue()
    {
        switch (itemType)
        {
            case ItemType.Bone:
                scoreValue = (boneVariant == BoneType.GoldenBone) ? 20 : 10;
                break;
            case ItemType.Poop:
                scoreValue = (poopVariant == PoopType.BigPoop) ? -20 : -10;
                break;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySFXSound(pickupSound,4f);
            
        }
    }
}
