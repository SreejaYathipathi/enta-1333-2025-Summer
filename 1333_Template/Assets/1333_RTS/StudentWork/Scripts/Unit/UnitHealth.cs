using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;

    [SerializeField] private HealthBarUI _healthBar;  // drag child HealthBar here

    private void Start()
    {
        _currentHealth = maxHealth;
        if (_healthBar != null)
            _healthBar.SetFill(1f);
    }

    public bool TakeDamage(float damage)
    {
        _currentHealth -= damage;
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
