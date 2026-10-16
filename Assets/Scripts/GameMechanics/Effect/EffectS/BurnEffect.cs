using UnityEngine;

[CreateAssetMenu(
    fileName = "NewBurnEffect",
    menuName = "Game/Effects/Burn"
)]
public class BurnEffect : EffectData
{
    [Header("Damage")]
    [Min(1)]
    [SerializeField] private int damagePerTurn = 10;

    public int DamagePerTurn =>
        damagePerTurn;

    public override void OnTurnStarted(
        ActiveEffect effect)
    {
        if (effect == null)
            return;

        Unit target =
            effect.Target?.Unit;

        if (target == null)
            return;

        int totalDamage =
            damagePerTurn *
            effect.StackCount;

        target.TakeDamage(totalDamage);
    }
}