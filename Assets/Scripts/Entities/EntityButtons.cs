using UnityEngine;

public class EntityButtons : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] buttonRenderers;

    public void SetColor(Color color)
    {
        foreach (var renderer in buttonRenderers)
        {
            if (renderer != null)
                renderer.material.color = color;
        }
    }

    public void SetActive(bool active)
    {
        foreach (var renderer in buttonRenderers)
        {
            if (renderer != null)
                renderer.gameObject.SetActive(active);
        }
    }
}