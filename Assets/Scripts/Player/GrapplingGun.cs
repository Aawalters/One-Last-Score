using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    [Header("Scripts Ref:")]
    public GrapplingRope grappleRope;

    [Header("Layers Settings:")]
    [SerializeField] private int grappableLayerNumber = 9;

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

    private void Start()
    {
        //default
        grappleRope.enabled = false;
        m_springJoint2D.enabled = false;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            // grab
            SetGrapplePoint();
        }
        else if (Input.GetKey(KeyCode.Mouse0) && grappleRope.enabled)
        {
            // rotation grappling
            RotateGun(grapplePoint);
        }
        else if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            // release
            grappleRope.enabled = false;
            m_springJoint2D.enabled = false;
        }
        else
        {
            // rotation not grappling
            Vector2 mousePos = m_camera.ScreenToWorldPoint(Input.mousePosition);
            RotateGun(mousePos);
        }
    }

    void RotateGun(Vector3 lookPoint)
    {
        Vector3 distanceVector = lookPoint - gunPivot.position;
        float angle = Mathf.Atan2(distanceVector.y, distanceVector.x) * Mathf.Rad2Deg;
        gunPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void SetGrapplePoint() {
        RaycastHit2D _hit = Physics2D.Raycast(firePoint.position, (m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized, maxDistance, grappableLayerNumber);
        //Debug.DrawRay(firePoint.position, ((m_camera.ScreenToWorldPoint(Input.mousePosition) - gunPivot.position).normalized * maxDistance), Color.red, maxDistance);
        if (_hit)
        {
            // Debug.Log("hit");
            Debug.Log("Hit object layer: " + _hit.collider.gameObject.layer);
            grapplePoint = _hit.point;
            grappleDistanceVector = grapplePoint - (Vector2)gunPivot.position; 
            grappleRope.enabled = true;
        }
    }


    public void Grapple()
    {
        m_springJoint2D.autoConfigureDistance = false;
        m_springJoint2D.connectedAnchor = grapplePoint;
        Vector2 distanceVector = firePoint.position - gunHolder.position;
        m_springJoint2D.distance = distanceVector.magnitude;
        m_springJoint2D.frequency = launchSpeed;
        m_springJoint2D.enabled = true;
        
    }
}
