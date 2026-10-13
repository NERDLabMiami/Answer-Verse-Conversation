using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VNEngine;

public class Checkpoint
{
    public Dictionary<string, bool> booleanStats;
    public Dictionary<string, float> numberedStats;
    public Dictionary<string, string> stringStats;

    public Checkpoint(Dictionary<string, bool> booleanStats, Dictionary<string, float> numberedStats, Dictionary<string, string> stringStats)
    {
        // Copy stats to ensure the checkpoint is an independent snapshot
        this.booleanStats = new Dictionary<string, bool>(booleanStats);
        this.numberedStats = new Dictionary<string, float>(numberedStats);
        this.stringStats = new Dictionary<string, string>(stringStats);
    }
}

public static class CheckpointManager
{
    // List of checkpoints
    public static List<Checkpoint> checkpoints = new List<Checkpoint>();

    // Save the current game state as a checkpoint
    public static void SaveCheckpoint()
    {
        // Save the current state of boolean, numbered, and string stats from StatsManager
        Checkpoint newCheckpoint = new Checkpoint(StatsManager.boolean_stats, StatsManager.numbered_stats, StatsManager.string_stats);

        checkpoints.Add(newCheckpoint);
        Debug.Log("Checkpoint saved. Total checkpoints: " + checkpoints.Count);
    }

    // Load a previous checkpoint based on index
    public static void LoadCheckpoint(int index)
    {
        if (index < checkpoints.Count)
        {
            Checkpoint checkpoint = checkpoints[index];

            // Restore boolean stats
            StatsManager.boolean_stats = new Dictionary<string, bool>(checkpoint.booleanStats);
            // Restore numbered stats
            StatsManager.numbered_stats = new Dictionary<string, float>(checkpoint.numberedStats);
            // Restore string stats
            StatsManager.string_stats = new Dictionary<string, string>(checkpoint.stringStats);

            Debug.Log("Loaded checkpoint " + index);
        }
        else
        {
            Debug.LogError("Invalid checkpoint index: " + index);
        }
    }

    // Display a list of checkpoints (for a UI system)
    public static void DisplayCheckpoints()
    {
        for (int i = 0; i < checkpoints.Count; i++)
        {
            Debug.Log("Checkpoint " + i + " is available for loading.");
            // Here, you can link this output to a button in your UI to select a checkpoint
        }
    }

    // Clear all checkpoints
    public static void ClearCheckpoints()
    {
        checkpoints.Clear();
        Debug.Log("All checkpoints cleared.");
    }
}

