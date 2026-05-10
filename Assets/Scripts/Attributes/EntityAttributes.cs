using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityAttributes : MonoBehaviour
{
    [Header("Entity Attributes")]
    [field: SerializeField] public List<BaseAttribute> AttributeList { get; protected set; } = new List<BaseAttribute>();

    [Header("Cached Attributes")]
    [field: SerializeField] public BaseAttribute Health { get; protected set; }
    [field: SerializeField] public BaseAttribute Mana { get; protected set; }
    [field: SerializeField] public BaseAttribute Energy { get; protected set; }
    
    [Header("EntityPower")]
    [field: SerializeField] public float EntityPower { get; protected set; }
    [Header("Skills Container")]
    [field: SerializeField] public Transform SkillsContainer { get; protected set; }

    // Dictionary for fast lookup by AttributeType
    private readonly Dictionary<string, BaseAttribute> _attributeDictionary = new Dictionary<string, BaseAttribute>();

    // Power system
    public event Action<float> OnPowerChanged;

    private List<PrimaryAttribute> _primaryAttributes = new List<PrimaryAttribute>();
    private bool _isSubscribed;

    private void Awake()
    {
        BuildAttributeDictionary();
        CachePrimaryAttributes();
    }

    private void Start()
    {
        // Set default base values for Primary attributes
        foreach (var attribute in _primaryAttributes)
        {
            attribute.SetBaseValue(10);
        }

        CalculateEntityPower(); // Initial calculation
    }

    private void CachePrimaryAttributes()
    {
        _primaryAttributes = GetAllAttributes<PrimaryAttribute>();
    }

    public void SetCanLevelUp(bool canLevelUp)
    {
        foreach (var attribute in AttributeList)
        {
            if (canLevelUp)
            {
                attribute.SetCanLevel(true);
            }
        }
    }

    public void SetAllLifeAttributeToMax()
    {
        foreach (var life in GetAllLifeAttributes())
        {
            life.SetCurrentValueToMaxValue();
        }
    }

    /// <summary>
    /// Calculates Entity Power as the average of all Primary Attributes' CurrentValue
    /// </summary>
    public void CalculateEntityPower()
    {
        if (_primaryAttributes.Count == 0)
        {
            EntityPower = 0f;
            OnPowerChanged?.Invoke(0f);
            return;
        }

        float total = 0f;
        foreach (var primary in _primaryAttributes)
        {
            total += primary.CurrentValue;
        }

        float newPower = total / _primaryAttributes.Count;

        // Only update and notify if value actually changed
        if (!Mathf.Approximately(EntityPower, newPower))
        {
            EntityPower = newPower;
            OnPowerChanged?.Invoke(newPower);
        }
    }

    private void OnPrimaryValueChanged()
    {
        CalculateEntityPower();
    }

    private void SubscribeToPrimaryAttributes()
    {
        if (_isSubscribed) return;

        foreach (var primary in _primaryAttributes)
        {
            if (primary != null)
            {
                primary.CurrentValueChanged += OnPrimaryValueChanged;
            }
        }
        _isSubscribed = true;
    }

    private void UnsubscribeFromPrimaryAttributes()
    {
        if (!_isSubscribed) return;

        foreach (var primary in _primaryAttributes)
        {
            if (primary != null)
            {
                primary.CurrentValueChanged -= OnPrimaryValueChanged;
            }
        }
        _isSubscribed = false;
    }

    /// <summary>
    /// Rebuilds the dictionary from the AttributeList. Call this if you modify the list at runtime.
    /// </summary>
    private void BuildAttributeDictionary()
    {
        _attributeDictionary.Clear();
        
        foreach (var attribute in AttributeList)
        {
            if (attribute != null && _attributeDictionary.TryAdd(attribute.AttributeName, attribute))
            {
            }
            else if (attribute == null)
            {
                Debug.LogWarning($"Null attribute found in {gameObject.name}'s AttributeList", this);
            }
            else
            {
                Debug.LogWarning($"Duplicate AttributeType {attribute.AttributeType} found on {gameObject.name}. Only the first will be accessible via GetAttribute.", this);
            }
        }
    }
    
    /// <summary>
    /// Adds or gets existing skill. Lazy creation from JSON.
    /// </summary>
    public SkillAttribute AddSkill(string skillName)
    {
        var existing = GetAttribute<SkillAttribute>(skillName);
        if (existing != null) return existing;

        return AttributeManager.Instance?.CreateAndAddSkill(this, skillName);
    }

    /// <summary>
    /// Internal method used by AttributeManager
    /// </summary>
    public void AddAttribute(BaseAttribute attribute)
    {
        if (attribute == null) return;

        AttributeList.Add(attribute);
        BuildAttributeDictionary(); // Refresh lookup

        // If it's primary, cache it
        if (attribute is PrimaryAttribute pa)
        {
            _primaryAttributes.Add(pa);
            pa.CurrentValueChanged += OnPrimaryValueChanged;
        }
    }
    

    public BaseAttribute GetAttribute(string attributeName)
    {
        if (_attributeDictionary.TryGetValue(attributeName, out BaseAttribute attribute))
        {
            return attribute;
        }
        
        var found = AttributeList.FirstOrDefault(a => a && a.AttributeName == attributeName);
        if (found)
        {
            _attributeDictionary[attributeName] = found;
            return found;
        }

        Debug.LogWarning($"Attribute of type {attributeName} not found on {gameObject.name}", this);
        return null;
    }

    public List<LifeAttribute> GetAllLifeAttributes()
    {
        return AttributeList.OfType<LifeAttribute>().ToList();
    }

    public List<T> GetAllAttributes<T>() where T : BaseAttribute
    {
        return AttributeList.OfType<T>().ToList();
    }

    public T GetAttribute<T>(string attributeName) where T : BaseAttribute
    {
        return GetAttribute(attributeName) as T;
    }

    public void RefreshAttributes()
    {
        BuildAttributeDictionary();
        CachePrimaryAttributes();
        UnsubscribeFromPrimaryAttributes();
        SubscribeToPrimaryAttributes();
        CalculateEntityPower();
    }

    private void OnEnable()
    {
        SubscribeToPrimaryAttributes();
    }

    private void OnDisable()
    {
        UnsubscribeFromPrimaryAttributes();
    }

    private void OnDestroy()
    {
        UnsubscribeFromPrimaryAttributes();
    }
}