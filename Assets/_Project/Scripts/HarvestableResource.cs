using System.Collections;
using UnityEngine;

public class HarvestableResource : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Lua";
    [SerializeField] private int amount = 1;
    [SerializeField] private bool harvestOnce = true;
    [SerializeField] private bool hideVisualAfterHarvest = true;
    [SerializeField] private bool createEmptyFieldPatch = true;
    [SerializeField] private bool respawnAfterHarvest = true;
    [SerializeField] private float respawnDelay = 30f;
    [SerializeField] private Color mudColor = new Color(0.36f, 0.25f, 0.12f);
    [SerializeField] private Color borderColor = new Color(0.46f, 0.32f, 0.14f);
    [SerializeField] private Color strawColor = new Color(0.82f, 0.62f, 0.25f);
    [SerializeField] private Color grassColor = new Color(0.32f, 0.62f, 0.18f);
    [SerializeField] private float emptyPatchThickness = 0.06f;
    [SerializeField] private float emptyPatchYOffset = 0.02f;
    [SerializeField] private float borderWidth = 0.22f;
    [SerializeField] private float borderHeight = 0.16f;
    [SerializeField] private int stubbleRows = 5;
    [SerializeField] private int stubbleColumns = 8;
    [SerializeField] private float stubbleHeight = 0.18f;
    [SerializeField] private float stubbleWidth = 0.035f;

    private bool harvested;
    private GameObject emptyFieldPatch;

    public string GetInteractionText()
    {
        return itemName;
    }

    public void Interact()
    {
        if (harvestOnce && harvested)
        {
            NotificationUI.ShowMessage("Đã thu hoạch rồi.");
            return;
        }

        if (InventorySystem.Instance == null)
        {
            NotificationUI.ShowMessage("Không tìm thấy InventorySystem.");
            return;
        }

        if (!InventorySystem.Instance.AddItem(itemName, Mathf.Max(1, amount)))
        {
            NotificationUI.ShowMessage("Túi đồ đã đầy.");
            return;
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPickup();
        }

        harvested = true;
        NotificationUI.ShowMessage("Đã thu hoạch " + itemName + " x" + Mathf.Max(1, amount) + ".");

        if (createEmptyFieldPatch)
        {
            CreateEmptyFieldPatch();
        }

        if (hideVisualAfterHarvest)
        {
            SetChildRenderersEnabled(false);
        }

        Collider harvestCollider = GetComponent<Collider>();
        if (harvestCollider != null)
        {
            harvestCollider.enabled = false;
        }

        if (respawnAfterHarvest)
        {
            StartCoroutine(RespawnAfterDelay());
        }
        else
        {
            enabled = false;
        }
    }

    private void SetChildRenderersEnabled(bool isEnabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer resourceRenderer in renderers)
        {
            resourceRenderer.enabled = isEnabled;
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (emptyFieldPatch != null)
        {
            Destroy(emptyFieldPatch);
            emptyFieldPatch = null;
        }

        SetChildRenderersEnabled(true);

        Collider harvestCollider = GetComponent<Collider>();
        if (harvestCollider != null)
        {
            harvestCollider.enabled = true;
        }

        harvested = false;
    }

    private void CreateEmptyFieldPatch()
    {
        if (emptyFieldPatch != null)
        {
            Destroy(emptyFieldPatch);
        }

        Bounds bounds = GetVisualBounds();
        float sizeX = Mathf.Max(1f, bounds.size.x);
        float sizeZ = Mathf.Max(1f, bounds.size.z);
        float groundY = bounds.min.y + emptyPatchYOffset;

        GameObject patchRoot = new GameObject(gameObject.name + "_Empty_Field");
        emptyFieldPatch = patchRoot;
        patchRoot.transform.position = Vector3.zero;
        patchRoot.transform.rotation = Quaternion.identity;
        if (transform.parent != null)
        {
            patchRoot.transform.SetParent(transform.parent, true);
        }

        Vector3 center = new Vector3(bounds.center.x, groundY, bounds.center.z);
        CreatePatchPart(patchRoot.transform, PrimitiveType.Cube, "Mud",
            center,
            new Vector3(sizeX, emptyPatchThickness, sizeZ),
            mudColor);

        float borderY = groundY + borderHeight * 0.5f;
        CreatePatchPart(patchRoot.transform, PrimitiveType.Cube, "Border_North",
            new Vector3(bounds.center.x, borderY, bounds.center.z + sizeZ * 0.5f),
            new Vector3(sizeX + borderWidth * 2f, borderHeight, borderWidth),
            borderColor);
        CreatePatchPart(patchRoot.transform, PrimitiveType.Cube, "Border_South",
            new Vector3(bounds.center.x, borderY, bounds.center.z - sizeZ * 0.5f),
            new Vector3(sizeX + borderWidth * 2f, borderHeight, borderWidth),
            borderColor);
        CreatePatchPart(patchRoot.transform, PrimitiveType.Cube, "Border_East",
            new Vector3(bounds.center.x + sizeX * 0.5f, borderY, bounds.center.z),
            new Vector3(borderWidth, borderHeight, sizeZ),
            borderColor);
        CreatePatchPart(patchRoot.transform, PrimitiveType.Cube, "Border_West",
            new Vector3(bounds.center.x - sizeX * 0.5f, borderY, bounds.center.z),
            new Vector3(borderWidth, borderHeight, sizeZ),
            borderColor);

        CreateStubbleGrid(patchRoot.transform, bounds, sizeX, sizeZ, groundY);
        CreateEdgeGrass(patchRoot.transform, bounds, sizeX, sizeZ, groundY);
    }

    private void CreateStubbleGrid(Transform parent, Bounds bounds, float sizeX, float sizeZ, float groundY)
    {
        int rows = Mathf.Max(1, stubbleRows);
        int columns = Mathf.Max(1, stubbleColumns);
        float startX = bounds.center.x - sizeX * 0.38f;
        float startZ = bounds.center.z - sizeZ * 0.35f;
        float stepX = sizeX * 0.76f / Mathf.Max(1, columns - 1);
        float stepZ = sizeZ * 0.70f / Mathf.Max(1, rows - 1);

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                float offsetX = Mathf.Sin((row + 1) * 12.9898f + column * 78.233f) * 0.06f;
                float offsetZ = Mathf.Cos(row * 37.719f + (column + 1) * 9.151f) * 0.06f;
                Vector3 position = new Vector3(
                    startX + stepX * column + offsetX,
                    groundY + stubbleHeight * 0.5f,
                    startZ + stepZ * row + offsetZ
                );

                GameObject stubble = CreatePatchPart(parent, PrimitiveType.Cylinder, "Stubble",
                    position,
                    new Vector3(stubbleWidth, stubbleHeight * 0.5f, stubbleWidth),
                    strawColor);
                stubble.transform.rotation = Quaternion.Euler(0f, (row * 37f + column * 19f) % 360f, 0f);
            }
        }
    }

    private void CreateEdgeGrass(Transform parent, Bounds bounds, float sizeX, float sizeZ, float groundY)
    {
        int grassCount = 12;
        float grassHeight = 0.16f;
        float grassWidth = 0.06f;

        for (int index = 0; index < grassCount; index++)
        {
            bool onNorthSouth = index % 2 == 0;
            float t = (index + 0.5f) / grassCount;
            float x = Mathf.Lerp(bounds.center.x - sizeX * 0.45f, bounds.center.x + sizeX * 0.45f, t);
            float z = Mathf.Lerp(bounds.center.z - sizeZ * 0.45f, bounds.center.z + sizeZ * 0.45f, t);

            if (onNorthSouth)
            {
                z = index % 4 == 0 ? bounds.center.z + sizeZ * 0.5f : bounds.center.z - sizeZ * 0.5f;
            }
            else
            {
                x = index % 4 == 1 ? bounds.center.x + sizeX * 0.5f : bounds.center.x - sizeX * 0.5f;
            }

            GameObject grass = CreatePatchPart(parent, PrimitiveType.Cube, "Grass_Tuft",
                new Vector3(x, groundY + grassHeight * 0.5f, z),
                new Vector3(grassWidth, grassHeight, grassWidth),
                grassColor);
            grass.transform.rotation = Quaternion.Euler(0f, index * 29f, 10f);
        }
    }

    private GameObject CreatePatchPart(Transform parent, PrimitiveType primitiveType, string partName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject part = GameObject.CreatePrimitive(primitiveType);
        part.name = partName;
        part.transform.SetParent(parent, true);
        part.transform.position = position;
        part.transform.rotation = Quaternion.identity;
        part.transform.localScale = scale;

        Collider partCollider = part.GetComponent<Collider>();
        if (partCollider != null)
        {
            partCollider.enabled = false;
        }

        Renderer partRenderer = part.GetComponent<Renderer>();
        if (partRenderer != null)
        {
            partRenderer.material = CreateMaterial(color);
        }

        return part;
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        return material;
    }

    private Bounds GetVisualBounds()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        bool hasBounds = false;
        Bounds combinedBounds = new Bounds(transform.position, Vector3.one);

        foreach (Renderer resourceRenderer in renderers)
        {
            if (resourceRenderer == null)
            {
                continue;
            }

            if (!hasBounds)
            {
                combinedBounds = resourceRenderer.bounds;
                hasBounds = true;
            }
            else
            {
                combinedBounds.Encapsulate(resourceRenderer.bounds);
            }
        }

        return combinedBounds;
    }
}
