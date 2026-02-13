using UnityEngine;
using UnityEngine.UI;
using Antymology.Agents;
using System.Linq;

namespace Antymology.UI
{
    /// <summary>
    /// Displays current genes for the queen and average worker genes.
    /// </summary>
    public class GeneDisplayUI : MonoBehaviour
    {
        public Text geneText;
        
        private string[] geneNames = new string[]
        {
            "Seek Mulch",
            "Random Move",
            "Special Move",
            "Eat Mulch",
            "Dig",
            "Share Health"
        };

        void Awake()
        {
            if (geneText == null)
                geneText = GetComponentInChildren<Text>();
            
            if (geneText == null)
            {
                Debug.LogError("GeneDisplayUI: Could not find Text component! Make sure there's a Text child object.");
            }
        }

        void Update()
        {
            if (SimulationManager.Instance == null || geneText == null) return;

            var queen = SimulationManager.Instance.Queen;
            var ants = SimulationManager.Instance.GetAllAnts();
            var workers = ants.Where(a => a is WorkerAnt).Cast<WorkerAnt>().ToList();

            System.Text.StringBuilder output = new System.Text.StringBuilder();
            output.Append("<b>=== CURRENT GENES ===</b>\n\n");

            // Queen genes
            if (queen != null)
            {
                output.Append("<color=yellow><b>QUEEN:</b></color>\n");
                for (int i = 0; i < queen.genes.Length; i++)
                {
                    output.AppendFormat("  {0}: {1:F2}\n", geneNames[i], queen.genes[i]);
                }
                output.AppendFormat("  Health: {0:F0}/{1:F0}\n", queen.health, queen.maxHealth);
                output.AppendFormat("  Nests: {0}\n\n", queen.nestBlocksPlaced);
            }
            else
            {
                output.Append("<color=red>Queen is dead</color>\n\n");
            }

            // Average worker genes
            if (workers.Count > 0)
            {
                output.Append("<color=cyan><b>WORKERS (average):</b></color>\n");
                float[] avgGenes = new float[6];
                foreach (var worker in workers)
                {
                    for (int i = 0; i < 6; i++)
                        avgGenes[i] += worker.genes[i];
                }
                
                for (int i = 0; i < 6; i++)
                    avgGenes[i] /= workers.Count;

                for (int i = 0; i < avgGenes.Length; i++)
                {
                    output.AppendFormat("  {0}: {1:F2}\n", geneNames[i], avgGenes[i]);
                }

                float avgHealth = workers.Average(w => w.health);
                output.AppendFormat("  Avg Health: {0:F0}\n", avgHealth);
                output.AppendFormat("  Count: {0}\n", workers.Count);
            }
            else
            {
                output.Append("<color=red>No workers alive</color>\n");
            }

            geneText.text = output.ToString();
        }
    }
}