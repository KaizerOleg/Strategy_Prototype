using System;
using Unity.Netcode;

public struct NetworkMapTile :
    INetworkSerializable,
    IEquatable<NetworkMapTile>
{
    public int Q;
    public int R;
    public int TerrainId;
    public float Height;
    public float Moisture;

    public NetworkMapTile(
        int q,
        int r,
        int terrainId,
        float height,
        float moisture)
    {
        Q = q;
        R = r;
        TerrainId = terrainId;
        Height = height;
        Moisture = moisture;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer)
        where T : IReaderWriter
    {
        serializer.SerializeValue(ref Q);
        serializer.SerializeValue(ref R);
        serializer.SerializeValue(ref TerrainId);
        serializer.SerializeValue(ref Height);
        serializer.SerializeValue(ref Moisture);
    }

    public bool Equals(NetworkMapTile other)
    {
        return Q == other.Q &&
               R == other.R &&
               TerrainId == other.TerrainId &&
               Height.Equals(other.Height) &&
               Moisture.Equals(other.Moisture);
    }
}