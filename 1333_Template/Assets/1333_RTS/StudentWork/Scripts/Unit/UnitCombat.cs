using UnityEngine;

public class UnitCombat : MonoBehaviour
{
    private UnitInstance _unit;
    private float _attackCooldown = 0f;

    private float AttackRate => _unit.UnitType.attackRate;

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

        // Check distance dynamically (attack even while moving)
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance > _unit.Range)
            return;

        // Stop moving if in range
        if (_unit.IsMoving)
            _unit.ForceStopMoving();

        if (_attackCooldown <= 0f)
        {
            UnitHealth health = target.GetComponent<UnitHealth>();
            if (health != null)
            {
                //bool destroyed = health.TakeDamage(_unit.Damage);

                float damageAmount = Random.Range(_unit.UnitType.minHp, _unit.UnitType.maxHp);
                bool destroyed = health.TakeDamage(damageAmount);
                if (destroyed)
                    _unit.ClearTargetUnit();
            }
            else
            {
                Destroy(target.gameObject);
                _unit.ClearTargetUnit();
            }

            _attackCooldown = 1f / _unit.UnitType.attackRate;
        }
    }

    private void TryAttackBuilding()
    {
        BuildingHealth target = _unit.GetTargetBuilding();
        if (target == null) return;

        if (_unit.IsMoving) return;

        if (!_unit.HasReachedDestination())
            return;

        if (_attackCooldown <= 0f)
        {
            Debug.Log($"{name} attacks building {target.name}");

            bool destroyed = target.TakeDamage(_unit.Damage);
            _attackCooldown = 1f / _unit.UnitType.attackRate;

            if (destroyed)
            {
                Debug.Log("Trying to get nearby building");
                _unit.ClearTargetBuilding();
                StartCoroutine(DelayedEvaluateTarget());
            }
        }
    }

    private System.Collections.IEnumerator DelayedEvaluateTarget()
    {
        yield return null; // Wait one frame
        _unit.EvaluateTarget();
    }
}
