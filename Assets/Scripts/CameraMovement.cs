using UnityEngine;

/// <summary>
/// Keeps the camera still while the player moves around inside one screen,
/// then slides it across to the neighbouring screen once the player crosses
/// an edge. The screen the camera starts on counts as screen 0.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform player;

    [Tooltip("Units per second the camera travels while changing screens. Raise it for an instant cut.")]
    [SerializeField] private float transitionSpeed = 40f;

    [Tooltip("World X of the far left of the level. The camera will not show past it.")]
    [SerializeField] private float levelLeftEdge = -8.9f;

    [Tooltip("World X of the far right of the level. The camera will not show past it.")]
    [SerializeField] private float levelRightEdge = 44.5f;

    private Camera cam;
    private float originX;
    private float targetX;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        originX = transform.position.x;
        targetX = originX;
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 position = transform.position;

        // Only choose a new screen once the previous move has finished, so a
        // player stood right on an edge cannot make the camera oscillate and
        // so the camera always travels one screen at a time
        if (Mathf.Approximately(position.x, targetX))
        {
            targetX = ScreenCentreFor(player.position.x);
        }

        position.x = Mathf.MoveTowards(position.x, targetX, transitionSpeed * Time.deltaTime);
        transform.position = position;
    }

    private float ScreenCentreFor(float playerX)
    {
        float width = ScreenWidth();
        float halfWidth = width * 0.5f;

        // Rounding puts the switch exactly on the boundary between two screens
        int index = Mathf.RoundToInt((playerX - originX) / width);
        float centre = originX + index * width;

        float min = levelLeftEdge + halfWidth;
        float max = levelRightEdge - halfWidth;

        // A level narrower than one screen has no room to scroll, so sit in the middle
        if (max < min)
            return (levelLeftEdge + levelRightEdge) * 0.5f;

        return Mathf.Clamp(centre, min, max);
    }

    /// <summary>
    /// Worked out each time rather than cached, because the aspect ratio
    /// changes whenever the game window is resized.
    /// </summary>
    private float ScreenWidth()
    {
        return cam.orthographicSize * 2f * cam.aspect;
    }
}
