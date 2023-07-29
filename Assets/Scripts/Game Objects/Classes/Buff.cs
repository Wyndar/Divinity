[System.Serializable]
public class Buff
{
    public enum TargetState
    {
        Default, Stealth, Taunt, Camouflage
    }
    public TargetState targetState;
    public bool hasArmor;
    public bool hasShield;
    public Buff()
    {
        targetState = TargetState.Default;
        hasArmor = false;
        hasShield = false;
    }
}