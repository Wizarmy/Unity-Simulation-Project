using System;
using UnityEngine;
using UnityEngine.UIElements;

public class AvatarBody : MonoBehaviour
{
    [Header("Avatar Visuals")]
    [field: SerializeField] public Transform BodyTransform { get; protected set; }
    [field: SerializeField] public Transform HeadTransform { get; protected set; }
    
    [Header("Visuals - Renderers")]
    [SerializeField] protected MeshRenderer bodyRenderer;
    [SerializeField] protected MeshRenderer headRenderer;
    [SerializeField] protected MeshRenderer noseRenderer;
    [SerializeField] protected EntityButtons entityButtons;

    [Header("Colors")]
    [SerializeField] protected EntityColor entityColors;
    
    [Header("Scale")]
    [SerializeField] protected float avatarScale;
    public float AvatarScale => avatarScale;
    
    public event Action<float> OnScaleChanged;
    

    public void InitializeAvatar(float scale, int team,bool useRandomColors = true )
    {
        if (useRandomColors)
            entityColors = EntityColor.Random(team != 0);
        else
            entityColors = team == 0 ? EntityColor.DefaultAlly : EntityColor.DefaultEnemy;
        
        ApplyColors();
        SetScale(scale);
        
        transform.rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);
    }
    
    public void ApplyColors()
    {
        if (bodyRenderer) bodyRenderer.material.color = entityColors.body;
        if (headRenderer) headRenderer.material.color = entityColors.head;
        if (noseRenderer) noseRenderer.material.color = entityColors.nose;
        if (entityButtons) entityButtons.SetColor(Color.Lerp(entityColors.body, Color.white, 0.25f));
    }
    
    public void SetScale(float newScale)
    {
        avatarScale = Mathf.Max(0.1f, newScale);
        transform.localScale = Vector3.one * avatarScale;
        OnScaleChanged?.Invoke(avatarScale);
    }
    
}
