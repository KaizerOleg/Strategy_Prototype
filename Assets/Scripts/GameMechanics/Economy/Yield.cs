using System;

[Serializable]
public struct Yield
{
    public int food;
    public int production;
    public int gold;
    public int science;

    public Yield(
        int food,
        int production,
        int gold,
        int science)
    {
        this.food = food;
        this.production = production;
        this.gold = gold;
        this.science = science;
    }

    public static Yield operator +(Yield a, Yield b)
    {
        return new Yield(
            a.food + b.food,
            a.production + b.production,
            a.gold + b.gold,
            a.science + b.science
        );
    }
}