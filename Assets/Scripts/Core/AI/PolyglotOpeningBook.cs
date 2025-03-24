using static MoveGenerator;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class PolyglotBook
{
    private struct Entry
    {
        public ulong Key;
        public ushort Move;
        public ushort Weight;
        public uint Learn;
    }

    private List<Entry> entries = new List<Entry>();

    public bool LoadFromFile(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        int entrySize = 16; // Each entry is 16 bytes

        for (int i = 0; i <= data.Length - entrySize; i += entrySize)
        {
            Entry entry = new Entry
            {
                Key = BitConverter.ToUInt64(data, i),
                Move = BitConverter.ToUInt16(data, i + 8),
                Weight = BitConverter.ToUInt16(data, i + 10),
                Learn = BitConverter.ToUInt32(data, i + 12)
            };

            entries.Add(entry);
        }

        // Sort by key for binary search
        entries.Sort((a, b) => a.Key.CompareTo(b.Key));
        return true;
    }

    public bool TryGetMove(ulong zobristKey, out Move move, out int weight)
    {
        move = new Move();
        weight = 0;

        // Binary search for the key
        int low = 0;
        int high = entries.Count - 1;

        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (entries[mid].Key < zobristKey)
                low = mid + 1;
            else if (entries[mid].Key > zobristKey)
                high = mid - 1;
            else
            {
                // Found matching position - collect all moves for this position
                List<(ushort move, ushort weight)> moves = new List<(ushort, ushort)>();

                // Look backward for more entries with the same key
                for (int i = mid; i >= 0 && entries[i].Key == zobristKey; i--)
                    moves.Add((entries[i].Move, entries[i].Weight));

                // Look forward for more entries with the same key
                for (int i = mid + 1; i < entries.Count && entries[i].Key == zobristKey; i++)
                    moves.Add((entries[i].Move, entries[i].Weight));

                if (moves.Count == 0)
                    return false;

                // Select a move based on weights
                int totalWeight = 0;
                foreach (var m in moves)
                    totalWeight += m.weight;

                int randomWeight = UnityEngine.Random.Range(0, totalWeight);
                int currentWeight = 0;

                foreach (var m in moves)
                {
                    currentWeight += m.weight;
                    if (randomWeight < currentWeight)
                    {
                        move = DecodeMove(m.move);
                        weight = m.weight;
                        return true;
                    }
                }

                // Fallback
                move = DecodeMove(moves[0].move);
                weight = moves[0].weight;
                return true;
            }
        }

        return false;
    }

    private Move DecodeMove(ushort polyglotMove)
    {
        // Polyglot move format:
        // bits 0-5: from square (0-63)
        // bits 6-11: to square (0-63)

        int fromSquare = polyglotMove & 0x3F;
        int toSquare = (polyglotMove >> 6) & 0x3F;

        return new Move(fromSquare, toSquare);
    }
}