using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;
    private HealthBarUI _healthBar;

    private void Start()
    {
        _currentHealth = maxHealth;
        _healthBar = FindObjectOfType<HealthBarUI>(true);
    }

    public bool TakeDamage(float damage)
    {
        if (_healthBar != null)
        {
            _healthBar.AttachTo(transform);
        }

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
