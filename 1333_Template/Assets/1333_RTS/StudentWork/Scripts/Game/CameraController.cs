using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private CameraControlActions _cameraActions;
    private InputAction _movement;
    private Transform _cameraTransform;

    //horizontal motion
    [SerializeField]
    private float _maxSpeed = 5f;
    private float _speed;
    [SerializeField]
    private float _acceleration = 10f;
    [SerializeField]
    private float _damping = 15f;

    //vertical motion
    [SerializeField]
    private float _stepSize = 2f;
    [SerializeField]
    private float _zoomDampaning = 7.5f;
    [SerializeField]
    private float _minHeight = 5f;
    [SerializeField]
    private float _maxHeight = 50f;
    [SerializeField]
    private float _zoomSpeed = 2f;

    //Rotation
    [SerializeField]
    private float _maxRoatationSpeed = 0.5f;

    //Screen edge motion
    [SerializeField]
    [Range(0f, 0.1f)]
    private float _edgeTolerance = 0.05f;
    [SerializeField]
    private bool _useScreenEdge = true;

    //update the position of camera
    private Vector3 _targetPosition;

    private float _zoomHeight;

    //track and maintain velocity with a rigidbody
    private Vector3 _horizontalVelocity;
    private Vector3 _lastPosition;

    //tracks where dragging action started
    Vector3 startDrag;

    private void Awake()
    {
        _cameraActions = new CameraControlActions();
        _cameraTransform = this.GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        _zoomHeight = _cameraTransform.localPosition.y;
        _cameraTransform.LookAt(this.transform);

        _lastPosition = this.transform.position;
        _movement = _cameraActions.Camera.Movement;

        _cameraActions.Camera.RotateCamera.performed += RotateCamera;

        _cameraActions.Camera.ZoomCamera.performed += ZoomCamera;

        _cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        _cameraActions.Camera.RotateCamera.performed -= RotateCamera;

        _cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;

        _cameraActions.Disable();
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentState == GameState.GameOver)
            return;

        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
        {
            transform.position = Vector3.zero;
        }


        GetKeyboardMovement();
        
        DragCamera();

        updateVelocity();
        UpdateCameraPosition();
        UpdateBasePosition();
    }

    private void updateVelocity()
    {
        _horizontalVelocity = (this.transform.position - _lastPosition) / Time.deltaTime;
        _horizontalVelocity.y = 0;
        _lastPosition = this.transform.position;
    }

    private void GetKeyboardMovement()
    {
        Vector3 _inputValue = _movement.ReadValue<Vector2>().x * GetCameraRight()
                            + _movement.ReadValue<Vector2>().y * GetCameraForward();

        _inputValue = _inputValue.normalized;

        if (_inputValue.sqrMagnitude > 0.1f)
        {
            _targetPosition += _inputValue;
        }
    }

    private Vector3 GetCameraRight()
    {
        Vector3 _right = _cameraTransform.right;
        _right.y = 0;
        return _right;
    }

    private Vector3 GetCameraForward()
    {
        Vector3 _forward = _cameraTransform.forward;
        _forward.y = 0;
        return _forward;
    }

    private void UpdateBasePosition()
    {
        if (float.IsNaN(_targetPosition.x) || float.IsNaN(_targetPosition.y) || float.IsNaN(_targetPosition.z))
            _targetPosition = Vector3.zero;

        if (float.IsNaN(_horizontalVelocity.x) || float.IsNaN(_horizontalVelocity.y) || float.IsNaN(_horizontalVelocity.z))
            _horizontalVelocity = Vector3.zero;

        // Validate transform before use
        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
            transform.position = Vector3.zero;

        if (_targetPosition.sqrMagnitude > 0.1f)
        {
            _speed = Mathf.Lerp(_speed, _maxSpeed, Time.deltaTime * _acceleration);
            transform.position += _targetPosition * _speed * Time.deltaTime;
        }
        else
        {
            _horizontalVelocity = Vector3.Lerp(_horizontalVelocity, Vector3.zero, Time.deltaTime * _damping);
            transform.position += _horizontalVelocity * Time.deltaTime;
        }

        _targetPosition = Vector3.zero;
    }

    private void RotateCamera(InputAction.CallbackContext inputValue)
    {
        if (!Mouse.current.middleButton.isPressed)
        {
            return;
        }

        float value = inputValue.ReadValue<Vector2>().x;
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + value * _maxRoatationSpeed, 0f);
    }

    private void ZoomCamera(InputAction.CallbackContext ctx)
    {
        float scroll = -ctx.ReadValue<Vector2>().y;
        if (Mathf.Abs(scroll) < 0.01f) return;

        float target = _zoomHeight + scroll * _stepSize;
        _zoomHeight = Mathf.Clamp(target, _minHeight, _maxHeight);
    }

    private void UpdateCameraPosition()
    {
        Vector3 zoomTarget = new Vector3(
            _cameraTransform.localPosition.x,
            _zoomHeight,
            _cameraTransform.localPosition.z);

        if (_zoomHeight > _minHeight + 0.001f)
        {
            float deltaY = _zoomHeight - _cameraTransform.localPosition.y;
            zoomTarget -= _zoomSpeed * deltaY * Vector3.forward;
        }

        _cameraTransform.localPosition = Vector3.Lerp(
            _cameraTransform.localPosition,
            zoomTarget,
            Time.deltaTime * _zoomDampaning);

        if (_cameraTransform.localPosition.y < _minHeight)
        {
            Vector3 p = _cameraTransform.localPosition;
            p.y = _minHeight;
            _cameraTransform.localPosition = p;
        }

        _cameraTransform.LookAt(transform);
    }

    private void DragCamera()
    {
        if (!Mouse.current.middleButton.isPressed) return;

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                startDrag = ray.GetPoint(distance);
            }
            else
            {
                _targetPosition += startDrag - ray.GetPoint(distance);
            }
        }
    }

    public void ResetCameraMovement()
    {
        _targetPosition = Vector3.zero;
        _horizontalVelocity = Vector3.zero;

        // Optional: reset actual camera position if needed
        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
        {
            transform.position = Vector3.zero;
        }
    }
}
