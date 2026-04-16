using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GearResizer : MonoBehaviour
{
    [Header("Settings")]
    [Range(6, 18)]
    [SerializeField] private int toothCount = 6;
    [SerializeField] private List<Sprite> gearSprites = new List<Sprite>();

    [Header("Interaction")]
    public bool isDraggable = true;

    [Header("Movement")]
    public float rotationSpeed = 0f;

    private SpriteRenderer spriteRenderer;

    public int ToothCount => toothCount / 2;
    public int ActualToothCount => toothCount;

    // Easy access to modifying the layer from the dragger
    public int SortingOrder
    {
        get { return spriteRenderer != null ? spriteRenderer.sortingOrder : 0; }
        set { if (spriteRenderer != null) spriteRenderer.sortingOrder = value; }
    }

    void OnValidate()
    {
        if (toothCount % 2 != 0) toothCount--;
        ApplySprite();
        ResetSortingOrder();
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(Random.value, Random.value, Random.value);
        ResetSortingOrder();
    }

    void Start()
    {
        ApplySprite();
    }

    void Update()
    {
        if (rotationSpeed != 0)
        {
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }

    public void ResetSortingOrder()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        // Default base sorting
        spriteRenderer.sortingOrder = 20 - toothCount;
    }

    void ApplySprite()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (gearSprites == null || gearSprites.Count == 0) return;

        int spriteIndex = (toothCount - 6) / 2;
        if (spriteIndex >= 0 && spriteIndex < gearSprites.Count)
        {
            spriteRenderer.sprite = gearSprites[spriteIndex];
        }
    }
}