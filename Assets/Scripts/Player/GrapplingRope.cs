using UnityEngine;

public class GrapplingRope : MonoBehaviour
{
    [Header("General References:")]
    public GrapplingGun grapplingGun;
    public LineRenderer my_lineRenderer;

    [Header("General Settings:")]
    [SerializeField] private int precision = 40;
    [Range(0, 20)] [SerializeField] private float straightenLineSpeed = 5;

    [Header("Rope Animation Settings:")]
    public AnimationCurve ropeAnimationCurve;
    [Range(0.01f, 4)] [SerializeField] private float StartWaveSize = 2;
    float waveSize = 0;

    [Header("Rope Progression:")]
    public AnimationCurve ropeProgressionCurve;
    [SerializeField] [Range(1, 50)] private float ropeProgressionSpeed = 1;

    float moveTime = 0;

    bool straightLine = true;

    private void OnEnable()
    {
        moveTime = 0;
        my_lineRenderer.positionCount = precision;
        waveSize = StartWaveSize;
        straightLine = false;

        LinePointsToFirePoint();

        my_lineRenderer.enabled = true;
    }

    private void OnDisable()
    {
        my_lineRenderer.enabled = false;
    }

    private void LinePointsToFirePoint()
    {
        for (int i = 0; i < precision; i++)
        {
            my_lineRenderer.SetPosition(i, grapplingGun.firePoint.position);
        }
    }

    private void Update()
    {
        moveTime += Time.deltaTime;
        DrawRope();
    }

    void DrawRope()
    {
        if (!straightLine)
        {
            if (my_lineRenderer.GetPosition(precision - 1).x == grapplingGun.grapplePoint.x)
            {
                straightLine = true;
            }
            else
            {
                DrawRopeWaves();
            }
        }
        else
        {
            if (!grapplingGun.isGrappling)
            {
                grapplingGun.isGrappling = true;
            }
            if (waveSize > 0)
            {
                waveSize -= Time.deltaTime * straightenLineSpeed;
                DrawRopeWaves();
            }
            else
            {
                waveSize = 0;

                if (my_lineRenderer.positionCount != 2) { my_lineRenderer.positionCount = 2; }

                DrawRopeNoWaves();
            }
        }
        // Lock all points of the line renderer to a specific z-plane (e.g., z = 0)
        for (int i = 0; i < my_lineRenderer.positionCount; i++)
        {
            Vector3 position = my_lineRenderer.GetPosition(i);
            position.z = 0; // Or the desired z-coordinate
            my_lineRenderer.SetPosition(i, position);
        }
    }

    void DrawRopeWaves()
    {
        for (int i = 0; i < precision; i++)
        {
            float delta = (float)i / ((float)precision - 1f);
            Vector2 offset = Vector2.Perpendicular(grapplingGun.grappleDistanceVector).normalized * ropeAnimationCurve.Evaluate(delta) * waveSize;
            Vector2 targetPosition = Vector2.Lerp(grapplingGun.firePoint.position, grapplingGun.grapplePoint, delta) + offset;
            Vector2 currentPosition = Vector2.Lerp(grapplingGun.firePoint.position, targetPosition, ropeProgressionCurve.Evaluate(moveTime) * ropeProgressionSpeed);

            my_lineRenderer.SetPosition(i, currentPosition);
        }
    }

    void DrawRopeNoWaves()
    {
        my_lineRenderer.SetPosition(0, grapplingGun.firePoint.position);
        my_lineRenderer.SetPosition(1, grapplingGun.grapplePoint);
    }
}
