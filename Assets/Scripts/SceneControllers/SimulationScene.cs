using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SimulationScene : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button returnToStartButton;
    [SerializeField] private Button combatToggleButton;
    [SerializeField] private Button testAddOneToCultButton;
    
    private void Awake()
    {
        SetupButtons();
    }

    private void Start()
    {
      GameManager.Instance.InitializeGame(true);
    }

    private void SetupButtons()
    {
        if (returnToStartButton != null)
            returnToStartButton.onClick.AddListener(OnReturnToStartClicked);
        
        if (combatToggleButton != null)
            combatToggleButton.onClick.AddListener(ToggleCombat);
        
        if (testAddOneToCultButton != null)
            testAddOneToCultButton.onClick.AddListener(TestButton);
    }

    private void TestButton()
    {
      //  EntityManager.Instance.EntityList[0].Attributes.GetAttribute("Cultivation").AddExperience(750000f);
      //  EntityManager.Instance.EntityList[0].Attributes.GetAttribute("EntityLevel").AddExperience(15f);
      
      EntityManager.Instance.EntityList[0].Attributes.Health.ChangeCurrentValueByAmount(-10);
    }
    
    
    private void ToggleCombat()
    {
       GameManager.Instance.ToggleInCombatState();

       if (combatToggleButton == null) return;
       var text = combatToggleButton.GetComponentInChildren<TMPro.TMP_Text>();
        if (text != null)
            text.text = GameManager.Instance.IsInCombat ? "Stop Combat" : "Start Combat";
    }
    
    private void OnDestroy()
    {
        if (returnToStartButton != null)
            returnToStartButton.onClick.RemoveListener(OnReturnToStartClicked);
        
    }
    
    private void OnReturnToStartClicked()
    {
        // Load the start scene using our SceneManager
        SceneManager.Instance.LoadScene("StartScene");
     
    }
    
}
