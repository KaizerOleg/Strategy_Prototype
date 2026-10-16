using UnityEngine;

[CreateAssetMenu(
    fileName = "NewWeaponsEffect",
    menuName = "Game/Effects/New Weapons"
)]
public class NewWeaponsEffect : EffectData
{
    [Header("Attack Bonus")]
    [Min(1)]
    [SerializeField] private int attackBonus = 5;

    public int GetAttackBonus(
        ActiveEffect effect)
    {
        if (effect == null)
            return 0;

        return attackBonus;
    }
}
