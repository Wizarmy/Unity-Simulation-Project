using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingLifeDisplayBar : MonoBehaviour
{
    [SerializeField] private Image displayBarBorderImage;
    [SerializeField] private Image displayBarBackgroundImage;
    [SerializeField] private Image displayBarImage;
    [SerializeField] private TMP_Text displayText;
    
    [SerializeField] private BaseAttribute attachedToAttribute;

    private Color _barColor;

    public void InitializeDisplayBar(BaseAttribute attachedAttribute)
    {
        attachedToAttribute = attachedAttribute;
        _barColor = attachedAttribute.AttributeName switch
        {
            "Health" => Color.red,
            "Mana" => Color.blue,
            "Energy" => Color.yellow,
            _ => Color.purple
        };
        
        displayBarImage.color = _barColor;
        displayBarBackgroundImage.color = _barColor;
        displayBarBorderImage.color = _barColor;
        attachedToAttribute.CurrentValueChanged += UpdateAttributeBarValue;
        attachedToAttribute.MaxValueChanged += UpdateAttributeBarValue;
        
        UpdateAttributeBarValue();
    }
    
    public void UpdateAttributeBarValue()
    {
        displayText.text = $"{TextUtility.FormatSmart(attachedToAttribute.CurrentValue,0)} / {TextUtility.FormatSmart(attachedToAttribute.MaxValue,0)}";
        displayBarImage.fillAmount = attachedToAttribute.CurrentValue / attachedToAttribute.MaxValue;
    }

    private void OnDestroy()
    {
        if (attachedToAttribute == null) return;
        attachedToAttribute.CurrentValueChanged -= UpdateAttributeBarValue;
        attachedToAttribute.MaxValueChanged -= UpdateAttributeBarValue;
    }
    
}
