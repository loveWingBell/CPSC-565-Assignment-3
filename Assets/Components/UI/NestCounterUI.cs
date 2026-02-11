// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using UnityEngine;
using UnityEngine.UI;
using Antymology.Agents;

namespace Antymology.UI
{
    public class NestCounterUI : MonoBehaviour
    {
        public Text uiText;

        void Awake()
        {
            if (uiText == null)
                uiText = GetComponent<Text>();
        }

        void Update()
        {
            if (SimulationManager.Instance == null || uiText == null) return;

            uiText.text =
                $"Generation:  {SimulationManager.Instance.CurrentGeneration}\n" +
                $"Nest Blocks: {SimulationManager.Instance.TotalNestBlocks}";
        }
    }
}