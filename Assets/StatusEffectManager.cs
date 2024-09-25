using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectManager : MonoBehaviour
{
    public RectTransform canvasRectTransform;
    public GameObject statusEffectPrefab;
    public float spacing = 10;

    private List<GameObject> statusEffects = new List<GameObject>();

    public void AddStatusEffect(Sprite EffectImage)
    {
        // Create a new status effect image from the prefab
        GameObject newStatusEffect = Instantiate(statusEffectPrefab, canvasRectTransform);
        newStatusEffect.GetComponent<Image>().sprite = EffectImage;
        
        RectTransform rectTransform = newStatusEffect.GetComponent<RectTransform>();


        float totalWidth = rectTransform.rect.width + spacing;
        float xOffset = -(statusEffects.Count * totalWidth);

        rectTransform.anchorMax = new Vector2(1, 1); // Top-right corner
        rectTransform.anchorMin = new Vector2(1, 1);
        rectTransform.pivot = new Vector2(1, 1);
        rectTransform.anchoredPosition = new Vector2(xOffset - 330, -48); // Adjust -200 for padding from top edge

        // Add the new image to the list
        statusEffects.Add(newStatusEffect);
    }

    public void RemoveStatusEffect(GameObject statusEffect)
    {
        statusEffects.Remove(statusEffect);
        Destroy(statusEffect);

        // Re-arrange the positions of the remaining status effects
        for (int i = 0; i < statusEffects.Count; i++)
        {
            float xOffset = -(i * (spacing));
            RectTransform rectTransform = statusEffects[i].GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(xOffset, -10);
        }
    }
}
