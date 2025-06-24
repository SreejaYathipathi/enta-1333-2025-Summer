using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;

    private HealthBarUI _healthBar;

    private void Start()
    {
        _currentHealth = maxHealth;

        _healthBar = FindObjectOfType<HealthBarUI>(true); // true = include inactive
        if (_healthBar != null)
        {
            _healthBar.AttachTo(transform);
            _healthBar.SetFill(1f);
        }
    }

    public void TakeDamage(float dmg)
    {
        _currentHealth -= dmg;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_healthBar != null)
            _healthBar.SetFill(_currentHealth / maxHealth);

        if (_currentHealth <= 0)
        {
            Debug.Log($"{name} is dead.");
            // Destroy(gameObject);
        }
    }
}