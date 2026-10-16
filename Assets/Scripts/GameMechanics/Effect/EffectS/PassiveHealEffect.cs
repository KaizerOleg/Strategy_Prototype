using UnityEngine;

[CreateAssetMenu(
    fileName = "NewPassiveHealEffect",
    menuName = "Game/Effects/Passive Heal"
)]
public class PassiveHealEffect : EffectData
{
    [Header("Healing")]
    [Min(1)]
    [SerializeField] private int healAmount = 10;

    public override void OnTurnStarted(
        ActiveEffect effect)
    {
        if (effect == null)
            return;

        Unit target =
            effect.Target?.Unit;

        if (target == null)
            return;

        target.Heal(healAmount);
    }
}