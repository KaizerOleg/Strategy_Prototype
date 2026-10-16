using System;
using System.Collections.Generic;

[Serializable]
public struct HexCoordinates
{
    public int Q;
    public int R;

    public HexCoordinates(int q, int r)
    {
        Q = q;
        R = r;
    }

    public int S
    {
        get
        {
            return -Q - R;
        }
    }

    public int GetDistance(HexCoordinates other)
    {
        int qDistance = Math.Abs(Q - other.Q);
        int rDistance = Math.Abs(R - other.R);
        int sDistance = Math.Abs(S - other.S);

        return Math.Max(
            qDistance,
            Math.Max(rDistance, sDistance)
        );
    }

    public List<HexCoordinates> GetNeighbors()
    {
        return new List<HexCoordinates>
        {
            new HexCoordinates(Q + 1, R),
            new HexCoordinates(Q - 1, R),

            new HexCoordinates(Q, R + 1),
            new HexCoordinates(Q, R - 1),

            new HexCoordinates(Q + 1, R - 1),
            new HexCoordinates(Q - 1, R + 1)
        };
    }

    public override string ToString()
    {
        return $"({Q}, {R})";
    }
}