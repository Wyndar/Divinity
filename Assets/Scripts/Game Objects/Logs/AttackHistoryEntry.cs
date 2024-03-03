[System.Serializable]
public class AttackHistoryEntry: GameLogHistoryEntry
{
	public CardLogic attacker;

	public AttackHistoryEntry(CardLogic attacker) => this.attacker = attacker;
}