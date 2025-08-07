using UnityEngine;

/// <summary>
/// Handles hit-points, damage, and death for a building.
/// Shows / hides a floating health-bar when damage is taken.
/// </summary>
public class BuildingHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public BuildingPurpose purpose;
    public int ArmyID = 0;

    [SerializeField] private HealthBarUI _healthBar;
    [SerializeField] private float hideDelay = 4f;

    private float _currentHealth;
    private float _lastDamageTime;
    public Vector2Int FootprintSize { get; set; }

    private void Start()
    {
        _currentHealth = maxHealth;
        if (_healthBar != null)
            _healthBar.SetFill(1f);
    }

    private void Update()
    {
        if (_healthBar != null && _healthBar.gameObject.activeSelf &&
            Time.time - _lastDamageTime >= hideDelay)
        {
            _healthBar.Hide();
        }
    }

    /// <summary>
    /// Apply damage returns true if building is destroyed.
    /// </summary>
    public bool TakeDamage(float dmg)
    {
        _currentHealth -= dmg;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_healthBar != null)
        {
            _healthBar.Show();
            _healthBar.SetFill(_currentHealth / maxHealth);
            _lastDamageTime = Time.time;
        }

        if (_currentHealth <= 0)
        {
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    /// <summary>Refills HP and hides the bar (used on repair / reset).</summary>
    public void ResetHealth()
    {
        _currentHealth = maxHealth;
        if (_healthBar != null)
        {
            _healthBar.SetFill(1f);
            _healthBar.Hide();   // optional
        }
    }
}
