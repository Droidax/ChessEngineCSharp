using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TranspositionTable
{
    public enum EntryFlag
    {
        Exact,
        LowerBound,
        UpperBound
    }

    public class TranspositionEntry
    {
        public ulong Key;
        public int Score;
        public int Depth;
        public EntryFlag Flag;
        public MoveGenerator.Move BestMove;
        public int Age; // Add an age field to track how recent this entry is
    }

    private static TranspositionTable instance;
    private readonly int maxTableSize; // Maximum number of entries
    private int currentAge; // Current search age

    // Use a replacement scheme - here we'll use a Dictionary for lookups
    // but manage its size manually
    private Dictionary<ulong, TranspositionEntry> table;

    private TranspositionTable(int sizeInMB)
    {
        // Estimate how many entries will fit in the given memory size
        // Each entry is roughly 32 bytes (ulong + 3 ints + enum + Move struct + overhead)
        int approximateEntrySize = 32;
        maxTableSize = (sizeInMB * 1024 * 1024) / approximateEntrySize;

        table = new Dictionary<ulong, TranspositionEntry>(maxTableSize);
        currentAge = 0;

        Debug.Log($"Transposition table initialized with max capacity of {maxTableSize} entries (~{sizeInMB}MB)");
    }

    public static TranspositionTable Instance
    {
        get
        {
            if (instance == null)
            {
                // Default to 64MB table size, adjust as needed
                instance = new TranspositionTable(64);
            }
            return instance;
        }
    }

    // Clear the table between games or when starting a new search
    public void Clear()
    {
        table.Clear();
        currentAge = 0;
        Debug.Log("Transposition table cleared");
    }

    // Increment age when starting a new search
    public void NewSearch()
    {
        currentAge++;
        // Optionally clear old entries when age cycles or gets too high
        if (currentAge > 100)
        {
            PruneOldEntries();
        }
    }

    private void PruneOldEntries()
    {
        // Remove entries from previous searches when table gets too large
        if (table.Count > maxTableSize * 0.9) // When we reach 90% capacity
        {
            List<ulong> keysToRemove = new List<ulong>();
            int threshold = currentAge - 3; // Keep only recent 3 searches

            foreach (var entry in table)
            {
                if (entry.Value.Age < threshold)
                {
                    keysToRemove.Add(entry.Key);
                }

                // Stop if we've found enough keys to remove
                if (keysToRemove.Count > maxTableSize / 3) break;
            }

            foreach (var key in keysToRemove)
            {
                table.Remove(key);
            }

            Debug.Log($"Pruned {keysToRemove.Count} old entries from transposition table");
        }
    }

    public void Store(ulong key, int score, int depth, EntryFlag flag, MoveGenerator.Move bestMove)
    {
        // Check if we need to make room in the table
        if (table.Count >= maxTableSize && !table.ContainsKey(key))
        {
            // Simple replacement scheme: remove a random entry
            // A more sophisticated approach would use a replacement strategy
            // like replacing the oldest or least valuable entry
            if (table.Count > 0)
            {
                ulong keyToRemove = default;
                int lowestDepth = int.MaxValue;
                int oldestAge = int.MaxValue;

                // Simple sampling to find a candidate for removal
                // Look at a few random entries and remove the shallowest/oldest
                int samplesToCheck = System.Math.Min(10, table.Count);
                int sampleCount = 0;

                foreach (var entry in table)
                {
                    if (sampleCount++ >= samplesToCheck) break;

                    // Prefer removing shallow entries from old searches
                    if (entry.Value.Age < oldestAge ||
                        (entry.Value.Age == oldestAge && entry.Value.Depth < lowestDepth))
                    {
                        keyToRemove = entry.Key;
                        lowestDepth = entry.Value.Depth;
                        oldestAge = entry.Value.Age;
                    }
                }

                if (keyToRemove != default)
                {
                    table.Remove(keyToRemove);
                }
            }
        }

        // Always prefer deeper searches or replace existing entries
        if (table.TryGetValue(key, out TranspositionEntry transpositionEntry))
        {
            // Replace if this is a deeper search or from a more recent search
            if (depth >= transpositionEntry.Depth || currentAge > transpositionEntry.Age)
            {
                transpositionEntry.Score = score;
                transpositionEntry.Depth = depth;
                transpositionEntry.Flag = flag;
                transpositionEntry.BestMove = bestMove;
                transpositionEntry.Age = currentAge;
            }
        }
        else
        {
            // Add new entry
            table[key] = new TranspositionEntry
            {
                Key = key,
                Score = score,
                Depth = depth,
                Flag = flag,
                BestMove = bestMove,
                Age = currentAge
            };
        }
    }

    public TranspositionEntry Retrieve(ulong key)
    {
        if (table.TryGetValue(key, out TranspositionEntry entry))
        {
            return entry;
        }
        return null;
    }

    // Get statistics about the table
    public string GetStats()
    {
        return $"Table size: {table.Count}/{maxTableSize} entries, Current age: {currentAge}";
    }
}