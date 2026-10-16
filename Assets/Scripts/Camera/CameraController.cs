using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HexGrid hexGrid;

    [Header("Edge Scrolling")]
    [SerializeField] private bool enableEdgeScrolling = true;
    [SerializeField] private float edgeSize = 20f;
    [SerializeField] private float edgeMoveSpeed = 15f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 15f;

    [Header("Drag")]
    [SerializeField] private float dragSpeed = 1f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError(
                "Camera component is missing."
            );
        }

        if (hexGrid == null)
        {
            Debug.LogError(
                "HexGrid is not assigned."
            );
        }
    }

    private void Update()
    {
        HandleZoom();
        HandleDrag();

        if (!IsDragging())
        {
            HandleEdgeScrolling();
        }

        WrapCameraPosition();
    }

    private void HandleZoom()
    {
        if (Mouse.current == null)
            return;

        Vector2 scroll =
            Mouse.current.scroll.ReadValue();

        if (scroll.y == 0f)
            return;

        cam.orthographicSize -=
            scroll.y * zoomSpeed * 0.01f;

        cam.orthographicSize =
            Mathf.Clamp(
                cam.orthographicSize,
                minZoom,
                maxZoom
            );
    }

    private void HandleDrag()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.rightButton.isPressed)
            return;

        Vector2 mouseDelta =
            Mouse.current.delta.ReadValue();

        float unitsPerPixel =
            (cam.orthographicSize * 2f)
            / Screen.height;

        Vector3 movement =
            new Vector3(
                mouseDelta.x,
                mouseDelta.y,
                0f
            );

        movement *=
            unitsPerPixel *
            dragSpeed;

        transform.position -= movement;
    }

    private bool IsDragging()
    {
        if (Mouse.current == null)
            return false;

        return Mouse.current.rightButton.isPressed;
    }

    private void HandleEdgeScrolling()
    {
        if (!enableEdgeScrolling)
            return;

        if (Mouse.current == null)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Vector3 direction = Vector3.zero;

        if (mousePosition.x <= edgeSize)
            direction.x -= 1f;

        if (mousePosition.x >= Screen.width - edgeSize)
            direction.x += 1f;

        if (mousePosition.y <= edgeSize)
            direction.y -= 1f;

        if (mousePosition.y >= Screen.height - edgeSize)
            direction.y += 1f;

        if (direction == Vector3.zero)
            return;

        direction.Normalize();

        transform.position +=
            direction *
            edgeMoveSpeed *
            Time.deltaTime;
    }

    private void WrapCameraPosition()
    {
        if (hexGrid == null)
            return;

        Vector3 position = transform.position;

        float mapWidth =
            hexGrid.MapMaxX -
            hexGrid.MapMinX;

        float mapHeight =
            hexGrid.MapMaxY -
            hexGrid.MapMinY;

        if (mapWidth <= 0f || mapHeight <= 0f)
            return;

        float halfHeight =
            cam.orthographicSize;

        float halfWidth =
            halfHeight * cam.aspect;

        float leftLimit =
            hexGrid.MapMinX - halfWidth;

        float rightLimit =
            hexGrid.MapMaxX + halfWidth;

        float bottomLimit =
            hexGrid.MapMinY - halfHeight;

        float topLimit =
            hexGrid.MapMaxY + halfHeight;

        if (position.x < leftLimit)
        {
            position.x =
                hexGrid.MapMaxX + halfWidth;
        }
        else if (position.x > rightLimit)
        {
            position.x =
                hexGrid.MapMinX - halfWidth;
        }

        if (position.y < bottomLimit)
        {
            position.y =
                hexGrid.MapMaxY + halfHeight;
        }
        else if (position.y > topLimit)
        {
            position.y =
                hexGrid.MapMinY - halfHeight;
        }

        transform.position = position;
    }
}