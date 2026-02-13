// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using UnityEngine;
using UnityEngine.UI;
using Antymology.Agents;

namespace Antymology.UI
{
    /// <summary>
    /// Creates floating health bars above each ant.
    /// </summary>
    public class AntHealthBarsUI : MonoBehaviour
    {
        private Camera mainCamera;

        void Start()
        {
            mainCamera = Camera.main;
        }

        void LateUpdate()
        {
            if (SimulationManager.Instance == null) return;

            // Clear old health bars
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            // Create health bars for each ant
            var ants = SimulationManager.Instance.GetAllAnts();
            foreach (var ant in ants)
            {
                if (ant == null) continue;
                CreateHealthBarFor(ant);
            }
        }

        void CreateHealthBarFor(AntBase ant)
        {
            // Create container
            GameObject barContainer = new GameObject($"{ant.gameObject.name}_HealthBar");
            barContainer.transform.SetParent(transform, false);
            barContainer.transform.position = ant.transform.position + Vector3.up * 1.5f;
            
            // Always face camera
            if (mainCamera != null)
                barContainer.transform.rotation = Quaternion.LookRotation(barContainer.transform.position - mainCamera.transform.position);

            // Add Canvas to this specific health bar
            Canvas barCanvas = barContainer.AddComponent<Canvas>();
            barCanvas.renderMode = RenderMode.WorldSpace;
            
            RectTransform barRect = barContainer.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(1f, 0.2f);
            barRect.localScale = Vector3.one * 0.5f;

            // Background panel
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(barContainer.transform, false);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Health fill
            GameObject fillObj = new GameObject("HealthFill");
            fillObj.transform.SetParent(bgObj.transform, false);
            Image fillImage = fillObj.AddComponent<Image>();
            
            float healthPercent = ant.health / ant.maxHealth;
            fillImage.color = healthPercent > 0.5f ? new Color(0f, 1f, 0f, 0.9f) : 
                              healthPercent > 0.25f ? new Color(1f, 1f, 0f, 0.9f) : 
                              new Color(1f, 0f, 0f, 0.9f);
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = new Vector2(0, 0);
            fillRect.anchorMax = new Vector2(healthPercent, 1);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            // Health text
            GameObject textObj = new GameObject("HealthText");
            textObj.transform.SetParent(barContainer.transform, false);
            
            Text healthText = textObj.AddComponent<Text>();
            healthText.text = Mathf.RoundToInt(ant.health).ToString();
            healthText.fontSize = 24;
            healthText.alignment = TextAnchor.MiddleCenter;
            healthText.color = Color.white;
            healthText.fontStyle = FontStyle.Bold;
            
            // Text outline for visibility
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(1, -1);
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(0, 0.3f);
            textRect.offsetMax = new Vector2(0, 0.5f);
        }
    }
}