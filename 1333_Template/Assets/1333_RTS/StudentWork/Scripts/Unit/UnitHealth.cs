using UnityEngine;

public class UnitHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float _currentHealth;
    private HealthBarUI _healthBar;
    private float _hideDelay = 2f; // time to hide after last attack

    private void Start()
    {
        _currentHealth = maxHealth;
        _healthBar = FindObjectOfType<HealthBarUI>(true);

        if (_healthBar != null)
        {
            _healthBar.AttachTo(transform);
            _healthBar.gameObject.SetActive(false); // start hidden
        }
    }

    public bool TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, maxHealth);

        if (_healthBar != null)
        {
            _healthBar.gameObject.SetActive(true);
            _healthBar.SetFill(_currentHealth / maxHealth);

            // reset hide timer
            CancelInvoke(nameof(HideHealthBar));
            Invoke(nameof(HideHealthBar), _hideDelay);
        }

        if (_currentHealth <= 0)
        {
            if (_healthBar != null)
                _healthBar.gameObject.SetActive(false); // hide when dead

            Destroy(gameObject);
            return true;
        }

        return false;
    }

    private void HideHealthBar()
    {
        if (_healthBar != null)
            _healthBar.gameObject.SetActive(false);
    }
}
