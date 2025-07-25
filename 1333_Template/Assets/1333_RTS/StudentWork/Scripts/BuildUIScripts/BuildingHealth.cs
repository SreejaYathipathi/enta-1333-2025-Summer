using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public BuildingPurpose purpose;

    private float _currentHealth;

    [SerializeField] private HealthBarUI _healthBar;

    [Header("Army Settings")]
    public int ArmyID = 0;

    public Vector2Int FootprintSize { get; set; }

    private void Awake()
    {
        var refData = GetComponent<BuildingItemReference>();
        if (refData != null && refData.Data != null)
        {
            FootprintSize = refData.Data.footprintSize;
            purpose = refData.Data.purpose;
        }
    }

    private void Start()
    {
        _currentHealth = maxHealth;

        if (FootprintSize == Vector2Int.zero)
        {
            var data = GetComponent<BuildingItemReference>();
            if (data != null)
                FootprintSize = data.Data.footprintSize;
        }

        _healthBar = GetComponentInChildren<HealthBarUI>(true);

    }


    public bool TakeDamage(float dmg)
    {
        if (_healthBar != null)
        {
            _healthBar.AttachTo(transform);
            _healthBar.SetFill(1f);
        }
        _currentHealth -= dmg;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_healthBar != null)
            _healthBar.SetFill(_currentHealth / maxHealth);

        if (_currentHealth <= 0)
        {
            Debug.Log($"{name} is dead.");
            Destroy(gameObject);
            return true; // indicate building was destroyed
        }

        return false; // still alive
    }
}
