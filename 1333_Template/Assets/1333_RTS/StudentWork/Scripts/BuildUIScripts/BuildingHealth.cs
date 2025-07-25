using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public BuildingPurpose purpose;
    public int ArmyID = 0;

    private float _currentHealth;
    public Vector2Int FootprintSize { get; set; }

    [SerializeField] private HealthBarUI _healthBar;  // drag child HealthBar here

    private void Start()
    {
        _currentHealth = maxHealth;
        if (_healthBar != null)
            _healthBar.SetFill(1f);
    }

    public bool TakeDamage(float dmg)
    {
        _currentHealth -= dmg;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_healthBar != null)
            _healthBar.SetFill(_currentHealth / maxHealth);

        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }
}
