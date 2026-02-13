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
        
            int nestsThisGen = 0;
            var queen = SimulationManager.Instance.Queen;
            if (queen != null)
            {
                nestsThisGen = queen.nestBlocksPlaced;
            }
            
            // Total = previous gens + current gen
            int totalNests = SimulationManager.Instance.TotalNestBlocks + nestsThisGen;

            string output = string.Format(
                "Generation: {0}\nTick: {1} / {2}\nNest Blocks: {3} (this gen: {4})\nSurvivors (last gen): {5}/{6}\nCurrent Ants Alive: {7}/{8}",
                SimulationManager.Instance.CurrentGeneration,
                SimulationManager.Instance.CurrentTick,
                SimulationManager.Instance.maxTicksPerGeneration,
                totalNests,
                nestsThisGen,
                SimulationManager.Instance.SurvivorsLastGen,
                total,
                alive,
                total
            );

            uiText.text = output;
        }
    }
}