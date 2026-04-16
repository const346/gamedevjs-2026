using UnityEngine;

public class GearDragger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float edgeSnapThreshold = 1.0f;
    [SerializeField] private float centerSnapThreshold = 0.5f; // Threshold for stacking gears

    private const float radiusMultiplier = 0.33f;

    private Camera mainCamera;
    private GearResizer selectedGear;
    private GearResizer[] allGears;
    private GearResizer lastSnappedGear;

    // Flag to know how we snapped
    private bool isCenterSnapped = false;

    void Start()
    {
        mainCamera = Camera.main;
        UpdateGearsList();
    }

    void Update()
    {
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        if (Input.GetMouseButtonDown(0))
        {
            UpdateGearsList();
            SelectClosestGear(mouseWorldPos);

            if (selectedGear != null)
            {
                selectedGear.rotationSpeed = 0f;
                // Visually bring the gear to the front while dragging
                selectedGear.SortingOrder = 100;
            }
        }
        else if (Input.GetMouseButton(0) && selectedGear != null)
        {
            selectedGear.transform.position = mouseWorldPos;
            ApplySnapping();
        }
        else if (Input.GetMouseButtonUp(0) && selectedGear != null)
        {
            FinalizeDrop();
        }
    }

    public void UpdateGearsList()
    {
        allGears = Object.FindObjectsByType<GearResizer>(FindObjectsSortMode.None);
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 screenPosition = Input.mousePosition;
        screenPosition.z = -mainCamera.transform.position.z;
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    private void SelectClosestGear(Vector3 mousePos)
    {
        float closestDistance = float.MaxValue;
        GearResizer closestMatch = null;

        foreach (GearResizer gear in allGears)
        {
            // Skip gears that are not allowed to be dragged
            if (!gear.isDraggable) continue;

            float distanceToMouse = Vector2.Distance(mousePos, gear.transform.position);
            float interactableRadius = gear.ToothCount * radiusMultiplier;

            if (distanceToMouse <= interactableRadius && distanceToMouse < closestDistance)
            {
                closestDistance = distanceToMouse;
                closestMatch = gear;
            }
        }
        selectedGear = closestMatch;
    }

    private void ApplySnapping()
    {
        GearResizer closestMatch = null;
        float minDistance = float.MaxValue;
        isCenterSnapped = false;

        foreach (GearResizer gear in allGears)
        {
            if (gear == selectedGear) continue;

            float dist = Vector2.Distance(selectedGear.transform.position, gear.transform.position);

            // 1. Check Center-to-Center Snap
            if (dist < centerSnapThreshold)
            {
                isCenterSnapped = true;
                closestMatch = gear;
                break; // Center snap has highest priority
            }

            // 2. Check Edge-to-Edge Snap
            float selectedRadius = selectedGear.ToothCount * radiusMultiplier;
            float targetRadius = gear.ToothCount * radiusMultiplier;
            float idealDist = selectedRadius + targetRadius;
            float edgeDist = Mathf.Abs(dist - idealDist);

            if (edgeDist < edgeSnapThreshold && edgeDist < minDistance)
            {
                minDistance = edgeDist;
                closestMatch = gear;
            }
        }

        lastSnappedGear = closestMatch;

        if (lastSnappedGear != null)
        {
            if (isCenterSnapped)
            {
                // Snap center to center
                selectedGear.transform.position = lastSnappedGear.transform.position;
                // Match rotation immediately to avoid visual jumps
                selectedGear.transform.rotation = lastSnappedGear.transform.rotation;
            }
            else
            {
                // Edge snap logic (Kinematics)
                float selectedRadius = selectedGear.ToothCount * radiusMultiplier;
                float targetRadius = lastSnappedGear.ToothCount * radiusMultiplier;
                float idealDist = selectedRadius + targetRadius;

                Vector3 dir = (selectedGear.transform.position - lastSnappedGear.transform.position).normalized;
                if (dir == Vector3.zero) dir = Vector3.right;

                selectedGear.transform.position = lastSnappedGear.transform.position + (dir * idealDist);

                float alpha = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float thetaA = lastSnappedGear.transform.eulerAngles.z;
                float teethA = lastSnappedGear.ActualToothCount;
                float teethB = selectedGear.ActualToothCount;

                float thetaB = alpha + 90f - (180f / teethB) + (alpha - thetaA - 90f) * (teethA / teethB);

                selectedGear.transform.rotation = Quaternion.Euler(0, 0, thetaB);
            }
        }
    }

    private void FinalizeDrop()
    {
        if (lastSnappedGear != null)
        {
            // --- Speed and Direction Logic ---
            if (isCenterSnapped)
            {
                selectedGear.rotationSpeed = lastSnappedGear.rotationSpeed;
            }
            else
            {
                float ratio = (float)lastSnappedGear.ActualToothCount / selectedGear.ActualToothCount;
                selectedGear.rotationSpeed = -lastSnappedGear.rotationSpeed * ratio;
            }

            // --- NEW: Relative Sorting Order Logic ---
            if (lastSnappedGear.ActualToothCount > selectedGear.ActualToothCount)
            {
                // Target has MORE teeth -> Dragged goes ABOVE target
                selectedGear.SortingOrder = lastSnappedGear.SortingOrder + 1;
            }
            else if (lastSnappedGear.ActualToothCount < selectedGear.ActualToothCount)
            {
                // Target has FEWER teeth -> Dragged goes BELOW target
                selectedGear.SortingOrder = lastSnappedGear.SortingOrder - 1;
            }
            else
            {
                // Same amount of teeth. Default to putting it slightly above.
                selectedGear.SortingOrder = lastSnappedGear.SortingOrder + 1;
            }
        }
        else
        {
            // Dropped in empty space
            selectedGear.rotationSpeed = 0f;

            // Reset to default sort order based on its own size
            selectedGear.ResetSortingOrder();
        }

        selectedGear = null;
        lastSnappedGear = null;
    }
}