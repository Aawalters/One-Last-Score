using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;

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

    [Header("Distance:")]
    [SerializeField] private float maxDistance = 20;

    [Header("Launching:")]
    [SerializeField] private float launchSpeed = 1;

    [HideInInspector] public Vector2 grapplePoint;
    [HideInInspector] public Vector2 grappleDistanceVector;

    public bool isGrappled = false;    // is player grappled to an object?

    public bool isPulling = false;

    private GameObject grappledObject;  // object we hit (could be enemy or other)

    private void Start()
    {
        //default
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;

    }

    private void Update()
    {
        if(isGrappled && grappleRope.enabled)
        {
            if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                grapplePoint = grappledObject.transform.position;
            }

        }

    }

    public void RotateGun(Vector3 lookPoint)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;
        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void SetGrapplePoint(bool isgrounded) {
        RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, (m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized, maxDistance, grappableLayerMask);
        //Debug.DrawRay(firePoint.position, ((m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized * maxDistance), Color.red, maxDistance);
        if (_hit)
        {
            // Debug.Log("Hit object layer: " + _hit.collider.gameObject.layer);
            grapplePoint = _hit.point;
            grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position; 
            grappleRope.enabled = true;
            grappledObject = _hit.collider.gameObject;
            if (!isgrounded)
            {
                Grab(grappleDistanceVector);
            }
        }
    }

    public void Grab(Vector2 grappleDistanceVector)
    {
        if (gunHolder.position.y < grapplePoint.y)
        {
            ConfigurePlayerSpring(true, grapplePoint, grappleDistanceVector.magnitude);
        }
    }

    public void PullPlayer()
    {
        // Debug.Log("PULL PLAYER");
        Vector2 distanceVector = firePoint.position - gunHolder.position;
        ConfigurePlayerSpring(true, grapplePoint, distanceVector.magnitude);
        isPulling = true;
    }

    public void StopPullingPlayer()
    {
        // Debug.Log("STOP PULLING PLAYER");
        Vector2 distanceVector = firePoint.position - gunHolder.position;
        Vector2 currentVelocity = gunHolder.gameObject.GetComponent<Rigidbody2D>().velocity;
        ConfigurePlayerSpring(false, grapplePoint, distanceVector.magnitude);
        gunHolder.gameObject.GetComponent<Rigidbody2D>().velocity = currentVelocity;
        isPulling = false;
    }

    public void PullEnemy()
    {
        // Debug.Log("PULL ENEMY");
        if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            SpringJoint2D enemySpringJoint = grappledObject.GetComponent<SpringJoint2D>();
            // Debug.Log("selected name: " + grappledObject.gameObject.name);
            // Debug.Log("enemySpringJoint" + enemySpringJoint);
            if (enemySpringJoint != null)
            {
                enemySpringJoint.connectedAnchor = firePoint.position;  // Connect the enemy to the player
                enemySpringJoint.distance = 0;  // Enemy will be pulled towards the player
                enemySpringJoint.enabled = true;
            }
        }
        else
        {
            Debug.Log("No enemy to pull or invalid target.");
        }

    }

    public void StopPullingEnemy()
    {
        if (grappledObject != null && grappledObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            SpringJoint2D enemySpringJoint = grappledObject.GetComponent<SpringJoint2D>();
            if (enemySpringJoint != null)
            {
                enemySpringJoint.enabled = false;
            }
        }
        else
        {
            Debug.Log("No enemy to release or invalid target.");
        }
    }

    private void ConfigurePlayerSpring(bool state, Vector2 anchorPoint, float distance)
    {
        if (state)
        {
            m_springJoint2D.autoConfigureDistance = false;
            m_springJoint2D.connectedAnchor = anchorPoint;
            m_springJoint2D.distance = distance;
            m_springJoint2D.frequency = launchSpeed;
            m_springJoint2D.enabled = state;
        }
        else
        {
            m_springJoint2D.enabled = state;
        }
    }

}
