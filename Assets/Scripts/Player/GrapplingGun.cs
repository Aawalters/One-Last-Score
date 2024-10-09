using UnityEngine;
using UnityEngine.UI;


public class GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;
    public PlayerController playerController;

    [Header("Player Ref:")]
    public Player p;

    [Header("Layers Settings:")]
    [SerializeField] private LayerMask grappableLayerMask;

    [Header("Main Camera:")]
    public Camera m_camera;

    [Header("Transform Ref:")]
    public Transform gunHolder;
    public Transform gunPivot;
    public Transform firePoint;

    [Header("Physics Ref:")]
    public SpringJoint2D m_springJoint2D;
    public Rigidbody2D m_rigidbody;

    [Header("Distance:")]
    [SerializeField] private float maxDistance = 20;

    private enum LaunchType
    {
        Transform_Launch,
        Physics_Launch
    }

    [Header("Launching:")]
    [SerializeField] private LaunchType launchType = LaunchType.Physics_Launch;
    [SerializeField] private float launchSpeed = 1;


    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;

    [HideInInspector] public bool isGrappling;
    private GameObject grappledObject;

    public Texture2D defaultCursor;
    public Texture2D specialCursor;

    public Image cursorImage; // Assign this in the Unity Inspector

    // Cursor Hotspot (adjust for the click point)
    public Vector2 cursorHotspot = Vector2.zero;
    // Track the current cursor to avoid setting it unnecessarily
    private Texture2D currentCursor;

    [Header("Grapple Forgiveness")]
    public float GrappleRadius; // set 'thickness' of grapple detection for easier use
    public LineRenderer GrappleIndicator;

    private void Start()
    {
        // Cursor.visible = false;
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;
        //playerController = GameObject.Find("Player Controller").GetComponent<PlayerController>();
        // p = playerController.p;
        // Cursor.SetCursor(defaultCursor, cursorHotspot, CursorMode.Auto);

        // configuring line renderer
        GrappleIndicator.positionCount = 2; // Two points (start and end)
        GrappleIndicator.startWidth = 0.1f; // Set the starting width
        GrappleIndicator.endWidth = 0.1f; // Set the ending width
    }

    private void Update()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        RotateGun(mousePos); ////////////////// same thing?
        updateCursorLook();
        // Debug.Log(grappledObject + "1");
        if (isGrappling && grappleRope.enabled)
        {
            GrappleIndicator.enabled = false;
            // Debug.Log(grappledObject + "2");
            if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy")) 
            {
                // Debug.Log(grappledObject + "3");
                grapplePoint = grappledObject.transform.position;
                
            }
            if (grappledObject == null)
            {
                stopGrappling(); // if enemy dies as player is grappling it
            }
        } else {
            GrappleIndicator.enabled = true;
            GrappleIndication();
        }
    }

    // shows where grapple target/latch onto (if doesn't exist on a surface, then can't be grappled)
    public void GrappleIndication() {
        Vector2 direction = (m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized;
        RaycastHit2D _hit = Physics2D.CircleCast(firePoint.position, GrappleRadius, direction, maxDistance, grappableLayerMask);

        // Update the LineRenderer to show the projected grapple point
        if (_hit) {
            Vector3 endPoint = _hit.point;
            endPoint += Vector3.up * 0.5f;
            GrappleIndicator.SetPosition(0, endPoint + Vector3.up * 0.7f); // setting first point to be 1 unit behind second point to cut line short
            GrappleIndicator.SetPosition(1, endPoint); // Show the grapple target
        } else {
            GrappleIndicator.enabled = false;
        }
    }

    public void pull()
    {
        if (grappleRope.enabled)
        {
            RotateGun(grapplePoint);
        } else {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            RotateGun(mousePos);  ///////////////// same thing?
        }

        if (isGrappling)
        {
            launch();
        }
    }

    public void stopPulling()
    {
        Vector2 releaseVelocity = m_rigidbody.velocity;
        m_springJoint2D.enabled = false;
        //m_rigidbody.gravityScale = 1;
        m_rigidbody.velocity = releaseVelocity; //momentum
        isGrappling = false;
    }

    public void SetSpring(bool isGrounded)
    {
        if (isGrappling && (gunHolder.position.y < grapplePoint.y) && !p.isGrounded && Mathf.Abs(gunHolder.GetComponent<Rigidbody2D>().velocity.x) < 5)
        {
            m_springJoint2D.autoConfigureDistance = false;
            m_springJoint2D.connectedAnchor = grapplePoint;
            Vector2 distanceVector = grapplePoint - (Vector2)gunHolder.position;
            m_springJoint2D.distance = distanceVector.magnitude;
            m_springJoint2D.frequency = 0;
            m_springJoint2D.enabled = true;
        } else if (!isGrappling || !(gunHolder.position.y < grapplePoint.y) || p.isGrounded)
        {
            stopPulling();
        }
    }

    private void RotateGun(Vector3 lookPoint)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;
        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetGrapplePoint() {
        Vector2 direction = (m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized;
        RaycastHit2D _hit = Physics2D.CircleCast(firePoint.position, GrappleRadius, direction, maxDistance, grappableLayerMask);

        if (_hit)
        {
            grapplePoint = _hit.point;
            grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position;
            grappleRope.enabled = true;
            grappledObject = _hit.collider.gameObject;
        }
    }

    public void stopGrappling()
    {
        grappleRope.enabled = false;
        stopPulling();
        isGrappling = false;
    }

    public void launch()
    {
        m_springJoint2D.autoConfigureDistance = false;
        m_springJoint2D.connectedAnchor = grapplePoint;

        if (launchType == LaunchType.Physics_Launch)
        {
            Vector2 distanceVector = firePoint.position - gunHolder.position;
            m_springJoint2D.distance = distanceVector.magnitude;
            m_springJoint2D.frequency = launchSpeed;
            m_springJoint2D.enabled = true;
        }
        else if (launchType == LaunchType.Transform_Launch)
        {
            m_rigidbody.gravityScale = 0;
            m_rigidbody.velocity = Vector2.zero;
            Vector2 firePointDistance = firePoint.position - gunHolder.localPosition;
            Vector2 targetPos = grapplePoint - firePointDistance;
            gunHolder.position = Vector2.Lerp(gunHolder.position, targetPos, Time.deltaTime * (launchSpeed + 4));
            m_springJoint2D.enabled = false;
        }

    }

    public void PullEnemy()
    {
        if (isGrappling)
        {
            if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                SpringJoint2D enemySpringJoint = grappledObject.GetComponent<SpringJoint2D>();
                if (enemySpringJoint != null)
                {
                    enemySpringJoint.connectedAnchor = firePoint.position;
                    enemySpringJoint.distance = 0;
                    enemySpringJoint.enabled = true;
                }
            }
        }
    }

    public void StopPullingEnemy()
    {
        if (isGrappling) 
        {
            if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                SpringJoint2D enemySpringJoint = grappledObject.GetComponent<SpringJoint2D>();
                if (enemySpringJoint != null)
                {
                    enemySpringJoint.enabled = false;
                }
            }
            stopPulling();
        }
    }

    private void updateCursorLook()
    {
        // Move the cursor image to follow the mouse position
        Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
        if (!grappleRope.enabled)
        {
            cursorImage.transform.position = Input.mousePosition; 
        } else {
            cursorImage.transform.position = m_camera.WorldToScreenPoint(grapplePoint);
        }

        // Calculate direction and angle to rotate cursor
        Vector2 aimDirection = (mousePos - (Vector2)firePoint.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        cursorImage.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90);

    }
}
