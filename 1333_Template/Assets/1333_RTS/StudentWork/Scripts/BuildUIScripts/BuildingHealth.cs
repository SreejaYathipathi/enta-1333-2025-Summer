using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public BuildingPurpose purpose;

    private float _currentHealth;

    private HealthBarUI _healthBar;

    public Vector2Int FootprintSize { get; set; }


    private void Start()
    {
        _currentHealth = maxHealth;

        if (FootprintSize == Vector2Int.zero)
        {
            var data = GetComponent<BuildingItemReference>();
            if (data != null)
                FootprintSize = data.Data.footprintSize;
        }

        _healthBar = FindObjectOfType<HealthBarUI>(true);
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