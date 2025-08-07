using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles attacking logic for a UnitInstance (melee or ranged).
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

    // Try to damage a target unit if in range.
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
                float damageAmount = _unit.UnitType.damage; // use damage stat
                bool destroyed = health.TakeDamage(damageAmount);
                StartCoroutine(FlashUnit(target.gameObject));
                if (destroyed)
                    _unit.ClearTargetUnit();
                XPManager.Instance.AddXP(10);
            }
            else
            {
                Destroy(target.gameObject);
                _unit.ClearTargetUnit();
                XPManager.Instance.AddXP(10);
            }

            _attackCooldown = 1f / AttackRate;
        }
    }

    // Try to damage a target building.
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

            bool destroyed = target.TakeDamage(_unit.UnitType.damage);
            StartCoroutine(FlashUnit(target.gameObject));
            _attackCooldown = 1f / AttackRate;

            if (destroyed)
            {
                Debug.Log("Trying to get nearby building");
                _unit.ClearTargetBuilding();
                StartCoroutine(DelayedEvaluateTarget());
            }
        }
    }

    // Brief red flash to indicate damage.
    private IEnumerator FlashUnit(GameObject unit)
    {
        if (unit == null) yield break;

        var renderers = unit.GetComponentsInChildren<Renderer>();
        List<Color> originalColors = new List<Color>();

        foreach (var rend in renderers)
        {
            if (rend != null)
                originalColors.Add(rend.material.color);
            else
                originalColors.Add(Color.white);
        }

        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.material.color = Color.red;
        }

        yield return new WaitForSeconds(0.1f);

        if (unit == null) yield break;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].material.color = originalColors[i];
        }
    }

    // Wait one frame then re-evaluate targets.
    private System.Collections.IEnumerator DelayedEvaluateTarget()
    {
        yield return null; // Wait one frame
        _unit.EvaluateTarget();
    }
}
