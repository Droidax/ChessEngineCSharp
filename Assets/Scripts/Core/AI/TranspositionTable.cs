using Assets.Scripts.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    }

    public static TranspositionTable instance;

    private Dictionary<ulong, TranspositionEntry> table = new Dictionary<ulong, TranspositionEntry>();
    
    public TranspositionTable()
    {
    }

    public static TranspositionTable Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new TranspositionTable();
            }

            return instance;
        }
    }

    public void Store(ulong key, int score, int depth, EntryFlag flag, MoveGenerator.Move bestMove)
    {
        if (table.TryGetValue(key, out TranspositionEntry entry))
        {
            if (depth >= entry.Depth)
            {
                entry.Score = score;
                entry.Depth = depth;
                entry.Flag = flag;
                entry.BestMove = bestMove;
            }
        }
        else
        {
            if (table.Count >= 900_000)
            {
                var firstKey = table.Keys.First();
                table.Remove(firstKey);
            }
            table[key] = new TranspositionEntry
            {
                Key = key,
                Score = score,
                Depth = depth,
                Flag = flag,
                BestMove = bestMove,
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

    public int GetSize()
    {
        return table.Count;
    }
}
