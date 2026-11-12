using UnityEngine;

public class BiomeCenter
{
    private BiomesEnum biome;
    private Vector2Int coordinates;

    public BiomeCenter(BiomesEnum b, Vector2Int coord)
    {
        this.biome = b;
        this.coordinates = coord;
    }

    public BiomeCenter(BiomesEnum b, int x, int z)
    {
        this.biome = b;
        this.coordinates = new Vector2Int(x, z);
    }

    public BiomeCenter()
    {
        this.biome = BiomesEnum.Plains;
        this.coordinates = new Vector2Int(0, 0);
    }

    public BiomesEnum GetBiome()
    {
        return biome;
    }

    public Vector2Int GetCoordinates()
    {
        return coordinates;
    }
}
