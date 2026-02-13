// CPSC 565 Assignment 3 - Elda Britu - 30158734 - February 13, 2025

using UnityEngine;
using UnityEngine.UI;
using Antymology.Agents;

namespace Antymology.UI
{
    /// <summary>
    /// Displays generation, nest blocks, tick count, and survivor count.
    /// </summary>
    public class NestCounterUI : MonoBehaviour
    {
        public Text uiText;

        void Awake()
        {
            if (uiText == null)
                uiText = GetComponent<Text>();
            
            if (uiText == null)
            {
                Debug.LogError("NestCounterUI: Could not find Text component!");
            }
        }

        void Update()
        {
            if (SimulationManager.Instance == null || uiText == null) return;

            int alive = SimulationManager.Instance.GetAllAnts().Count;
            int total = SimulationManager.Instance.workerCount + 1;

            string output = string.Format(
                "Generation: {0}\nTick: {1} / {2}\nNest Blocks: {3}\nSurvivors (last gen): {4}/{5}\nCurrent Ants Alive: {6}/{7}",
                SimulationManager.Instance.CurrentGeneration,
                SimulationManager.Instance.CurrentTick,
                SimulationManager.Instance.maxTicksPerGeneration,
                SimulationManager.Instance.GetCurrentNestCount(),
                SimulationManager.Instance.SurvivorsLastGen,
                total,
                alive,
                total
            );

            uiText.text = output;
        }
    }
}