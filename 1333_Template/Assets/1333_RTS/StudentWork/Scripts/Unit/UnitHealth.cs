using UnityEngine;


// Handles a unit's health system, including taking damage, resetting health, 
// and showing/hiding the health bar UI.
public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] private HealthBarUI _healthBar;
    [SerializeField] private float hideDelay = 4f;

    private float _currentHealth;
    private float _lastDamageTime;

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

    // Reduces health by a given damage amount, updates UI, and destroys unit if health reaches zero
    public bool TakeDamage(float damage)
    {
        _currentHealth -= damage;
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

    // Restores health to max and hides the health bar
    public void ResetHealth()
    {
        _currentHealth = maxHealth;
        if (_healthBar != null)
        {
            _healthBar.SetFill(1f);
            _healthBar.Hide();   // if you still want bars hidden after reset
        }
    }
}
