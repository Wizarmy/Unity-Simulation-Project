using System;
using TMPro;
using UnityEngine;
using UnityEngine.Shaders;

public class AttributeInfoPanel : MonoBehaviour
{
   [Header("TextBoxes")]
   [field: SerializeField] public TMP_Text AttributeNameText { get; private set; }
   [field: SerializeField] public TMP_Text CurrentValueText{ get; private set; }
   [field: SerializeField] public TMP_Text MaxValueText{ get; private set; }
   [field: SerializeField] public TMP_Text FlatBonusValueText{ get; private set; }
   [field: SerializeField] public TMP_Text PercentBonusValueText{ get; private set; }
   [field: SerializeField] public TMP_Text LevelText{ get; private set; }
   [field: SerializeField] public TMP_Text ExpText{ get; private set; }
   
   [Header("Display Attribute")]
   [field: SerializeField] public BaseAttribute DisplayAttribute { get; private set; }
   private int _decimalPlaces;

   private bool _isLabelDisplay;
   
   public void InitializeAttributeInfoPanel(BaseAttribute attribute, bool labelDisplay = false)
   {
      UnsubscribeEvents(); // Always clean first

      if (labelDisplay)
      {
         SetAsLabelDisplay();
      }
      else
      {
         DisplayAttribute = attribute;
         AttributeNameText.text = DisplayAttribute.AttributeName;

         DisplayAttribute.CurrentValueChanged += UpdateDisplay;
         DisplayAttribute.MaxValueChanged += UpdateDisplay;
         DisplayAttribute.AttributeLevel.OnExpChanged += UpdateDisplay;
         DisplayAttribute.AttributeLevel.OnLevelUp += UpdateDisplay;

         _decimalPlaces = DisplayAttribute.AttributeType switch
         {
            AttributeTypeEnum.Cultivation => 0,
            AttributeTypeEnum.EntityLevel => 0,
            AttributeTypeEnum.Primary => 1,
            AttributeTypeEnum.Regen => 2,
            AttributeTypeEnum.Life => 1,
            AttributeTypeEnum.Derived => 2,
            AttributeTypeEnum.Resist => 1,
            AttributeTypeEnum.Damage => 1,
            _ => throw new ArgumentOutOfRangeException()
         };

         UpdateDisplay();
      }
   }

   private void SetAsLabelDisplay()
   {
      AttributeNameText.text = "Attribute";
      CurrentValueText.text = "Current";
      MaxValueText.text = "Max";
      FlatBonusValueText.text = "Flat";
      PercentBonusValueText.text = "%";
      LevelText.text = "Level";
      ExpText.text = "Exp";
   }

   public void UpdateDisplay()
   {
      if (!DisplayAttribute) return;
      CurrentValueText.text =TextUtility.FormatSmart(DisplayAttribute.CurrentValue,_decimalPlaces);
      MaxValueText.text = TextUtility.FormatSmart(DisplayAttribute.MaxValue,_decimalPlaces);
      FlatBonusValueText.text = TextUtility.FormatSmart(DisplayAttribute.FlatModifierValue);
      PercentBonusValueText.text = TextUtility.FormatSmart(DisplayAttribute.PercentModifierValue);
      if (DisplayAttribute.AttributeCanLevel)
      {
         LevelText.text = TextUtility.FormatSmart(DisplayAttribute.AttributeLevel.CurrentLevel);
         ExpText.text = $"{TextUtility.FormatSmart(DisplayAttribute.AttributeLevel.CurrentLevelExp)} / {TextUtility.FormatSmart(DisplayAttribute.AttributeLevel.ExpToNextLevel)}";   
      }
      else
      {
         LevelText.text = "N/A";
         ExpText.text = "N/A";
      }
      
   }
   
   public void UnsubscribeEvents()
   {
      if (DisplayAttribute == null) return;

      DisplayAttribute.CurrentValueChanged -= UpdateDisplay;
      DisplayAttribute.MaxValueChanged -= UpdateDisplay;
      DisplayAttribute.AttributeLevel.OnExpChanged -= UpdateDisplay;
      DisplayAttribute.AttributeLevel.OnLevelUp -= UpdateDisplay;

      DisplayAttribute = null;
   }
}
