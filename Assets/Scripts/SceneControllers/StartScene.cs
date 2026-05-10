using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider friendsSlider;
    [SerializeField] private Slider enemiesSlider;
    
    [Header("Toggles")]
    [SerializeField] private Toggle startWithMainCharacterToggle;

    [Header("Value Displays")]
    [SerializeField] private TMP_Text friendsValueText;
    [SerializeField] private TMP_Text enemiesValueText;

    [Header("Buttons")]
    [SerializeField] private Button startSimulationButton;
    
    private void Awake()
    {
        SetupSliders();
        SetupToggle();
        UpdateTextDisplays();           // Initial display

        if (startSimulationButton != null)
            startSimulationButton.onClick.AddListener(StartSimulation);
    }
    
    private void SetupSliders()
    {
        // Friends Slider
        if (friendsSlider != null)
        {
            friendsSlider.onValueChanged.AddListener(OnFriendsValueChanged);
            friendsSlider.minValue = 0;
            friendsSlider.maxValue = 30;        // Adjust max as desired
            friendsSlider.wholeNumbers = true;
        }

        // Enemies Slider
        if (enemiesSlider == null) return;
        enemiesSlider.onValueChanged.AddListener(OnEnemiesValueChanged);
        enemiesSlider.minValue = 1;
        enemiesSlider.maxValue = 30;
        enemiesSlider.wholeNumbers = true;
    }
    
    private void SetupToggle()
    {
        if (startWithMainCharacterToggle != null)
        {
            // Default to true (most players will want a main character)
            startWithMainCharacterToggle.isOn = true;
        }
    }
    
    private void OnFriendsValueChanged(float value)
    {
        UpdateTextDisplays();
    }

    private void OnEnemiesValueChanged(float value)
    {
        UpdateTextDisplays();
    }
    
    private void UpdateTextDisplays()
    {
        if (friendsValueText != null && friendsSlider != null)
            friendsValueText.text = $"Friends: {friendsSlider.value:F0}";

        if (enemiesValueText != null && enemiesSlider != null)
            enemiesValueText.text = $"Enemies: {enemiesSlider.value:F0}";
    }
    
    private void StartSimulation()
    {
        SimulationSettings.FriendsCount = friendsSlider != null ? Mathf.RoundToInt(friendsSlider.value) : 5;
        SimulationSettings.EnemiesCount = enemiesSlider != null ? Mathf.RoundToInt(enemiesSlider.value) : 3;
        SimulationSettings.StartWithMainCharacter = startWithMainCharacterToggle != null && startWithMainCharacterToggle.isOn;

        //Load SimulationScene
        // Load the scene using our SceneManager
        SceneManager.Instance.LoadScene("SimulationScene");
    }
    
    private void OnDestroy()
    {
        if (friendsSlider != null)
            friendsSlider.onValueChanged.RemoveListener(OnFriendsValueChanged);

        if (enemiesSlider != null)
            enemiesSlider.onValueChanged.RemoveListener(OnEnemiesValueChanged);

        if (startSimulationButton != null)
            startSimulationButton.onClick.RemoveListener(StartSimulation);
    }
    
}
