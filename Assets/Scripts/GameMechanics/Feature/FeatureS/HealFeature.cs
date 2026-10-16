using UnityEngine;

[CreateAssetMenu(
    fileName = "NewHealFeature",
    menuName = "Game/Features/Heal"
)]
public class HealFeature : Feature
{
    [Header("Heal")]
    [SerializeField] private int healAmount = 20;

    public int HealAmount =>
        healAmount;

    public override bool CanActivate(
        FeatureContext context)
    {
        if (context == null)
            return false;

        if (context.Unit == null)
            return false;

        return context.Unit.CurrentHealth <
               context.Unit.Data.maxHealth;
    }

    public override void Activate(
        FeatureContext context)
    {
        if (!CanActivate(context))
            return;

        Unit unit =
            context.Unit;

        unit.Heal(
            healAmount
        );
    }
}