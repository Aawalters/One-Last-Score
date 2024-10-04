using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonsAndClick : MonoBehaviour
{
    public Player p;
    public GameObject jumpButton;
    public GameObject kickButton;
    public bool isJumping = false;
    public bool isKicking = false;
    public bool pulling = false;
    public bool drawCard = false;
    public bool pause = false;
    // Start is called before the first frame update
    public void Start()
    {

    }

    public void jumpClick()
    {
        isJumping = true;
    }

    public void kickClick()
    {
        isKicking = true;
    }

    public void drawCardClick()
    {
        drawCard = true;
    }

    public void pauseClick()
    {
        pause = true;
    }

    public void grappleClick()
    {
        p.grapplingGun.SetGrapplePoint();
        pulling = true;
    }

    public void grappleRelease()
    {
        p.grapplingGun.stopGrappling();
        pulling = false;
    }
}
