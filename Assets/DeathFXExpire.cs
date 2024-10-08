using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DeathFXExpire : MonoBehaviour
{
    public float TimeToExpire;
    public float yPosChangeRate; // how fast skull floats upward into nothingness 
    public AnimationCurve _Curve;
    private SpriteRenderer _Rend;
    public bool shouldYPosChange;

    void Awake() {
        _Rend = GetComponent<SpriteRenderer>();
    }   
    
    void Start() {
        StartCoroutine(PlayFX());
    }

    private IEnumerator PlayFX() {
        // opacity tied to curve, expires once time ends
        float elapsedTime = 0f;
        while (elapsedTime < TimeToExpire) {
            elapsedTime += Time.deltaTime;
            float strength = _Curve.Evaluate(elapsedTime / TimeToExpire);
            _Rend.color = new Color(_Rend.color.r, _Rend.color.g, _Rend.color.b, strength); 
            if (shouldYPosChange)
                transform.position = new Vector2(transform.position.x, transform.position.y + yPosChangeRate);
            yield return null;
        }
        
        Destroy(this);
    }
}
