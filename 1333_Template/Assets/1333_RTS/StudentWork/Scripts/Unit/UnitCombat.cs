using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private UnitInstance _unit;
    private float _attackCooldown = 0f;

    [SerializeField] private float _attackRate = 1f; // Attacks per second

    private void Awake()
    {
        _unit = GetComponent<UnitInstance>();
    }

    private void Update()
    {
        _attackCooldown -= Time.deltaTime;

        if (_unit == null || (!_unit.HasTargetUnit() && !_unit.HasTargetBuilding()))
            return;

        if (_unit.HasTargetUnit())
        {
            TryAttackUnit();
        }
        else if (_unit.HasTargetBuilding())
        {
            TryAttackBuilding();
        }
    }

    private void TryAttackUnit()
    {
        UnitInstance target = _unit.GetTargetUnit();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _unit.Range)
        {
            _unit.MoveToPosition(target.transform.position); // Ask UnitInstance to path toward
            return;
        }

        if (_attackCooldown <= 0f)
        {
            Debug.Log($"{name} attacks unit {target.name}");
            Destroy(target.gameObject); // placeholder
            _unit.ClearTargetUnit();
            _attackCooldown = 1f / _attackRate;
        }
    }

    private void TryAttackBuilding()
    {
        BuildingHealth target = _unit.GetTargetBuilding();
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _unit.Range)
        {
            _unit.MoveToPosition(target.transform.position);
            return;
        }

        if (_attackCooldown <= 0f)
        {
            Debug.Log($"{name} attacks building {target.name}");
            target.TakeDamage(_unit.Damage);
            _attackCooldown = 1f / _attackRate;

            if (target == null || target.gameObject == null)
                _unit.ClearTargetBuilding();
        }
    }
}
