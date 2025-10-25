using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody), typeof(ShipCollisionHandler), typeof(ShipController))]
public class PlayerShooter : Shooter
{
    public ShipCollisionHandler shipCollisionHandler;
    public ShipController shipController;
    public LayerMask targetMask;

    private Camera mainCamera;
    private InputAction touchAction;

    public override void Init(Color laserColor, float aimError, float shootTimer)
    {
        shipCollisionHandler = GetComponent<ShipCollisionHandler>();
        shipController = GetComponent<ShipController>();
        mainCamera = CameraTPS.Instance.GetComponent<Camera>();
        base.Init(laserColor, aimError, shootTimer);
    }

    protected override bool CanShoot() => target != null && !shipCollisionHandler.isDestabilized && !shipController.IsMoving;

    protected override bool LoopCondition() => weaponHoles.Count > 0;

    void Awake()
    {
        // configuration de l’input (clic / touch)
        touchAction = new InputAction(type: InputActionType.PassThrough, binding: "<Pointer>/press");
        touchAction.Enable();
        touchAction.performed += ctx => OnTouch();
    }

    void OnDestroy()
    {
        touchAction.performed -= ctx => OnTouch();
        touchAction.Disable();
    }

    void OnTouch()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit, 500f, targetMask))
        {
            bool isEnemy = hit.collider.CompareTag("Enemy");
            bool isObstacle = hit.collider.CompareTag("Obstacle");

            if (isEnemy || isObstacle)
            {
                target = Utils.GetTopMostParent(hit.collider.transform);
                // // Toggle du lock
                // if (lockedTarget == hit.transform)
                // {
                //     ClearLock();
                // }
                // else
                // {
                //     LockTarget(hit.transform);
                // }
            }
        }
        else
        {
            Debug.Log("Clic dans le vide");
            // ClearLock(); // clic dans le vide
        }
    }
}
