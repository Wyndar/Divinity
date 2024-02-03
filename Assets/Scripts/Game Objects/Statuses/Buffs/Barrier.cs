using UnityEngine;

[System.Serializable]
public class Barrier : Buff
{
    public Barrier(CardLogic bufferLogic, CardLogic buffedLogic, int amount)
    {
        toolTipManager = GameObject.FindObjectOfType<ToolTipManager>();
        uIManager = GameObject.FindObjectOfType<UIManager>();
        toolTipInfo = toolTipManager.tooltipInfos.Find(a => a.key == "Barrier");
        buff = Buffs.Barrier;
        applierLogic = bufferLogic;
        affectedLogic = buffedLogic;
        Amount = amount;
        sprite = uIManager.barrierSprite;
    }

    public void DamageResolution(int damage, bool wasAttack)
    {
        int outgoingDamage = damage - Amount > 0 ? damage - Amount : 0;
        CombatantLogic combatant = affectedLogic.GetComponent<CombatantLogic>();
        Amount -= damage;
        if (Amount < 1)
        {
            if (fieldIconHolder != null)
            {
                fieldIconHolder.durationText.text = Timer.ToString();
                if (Timer <= 0)
                {
                    fieldIconHolder.transform.SetParent(null);
                    fieldIconHolder.DestroyThis();
                }
            }
            combatant.cardStatuses.Remove(this);
        }
        uIManager.StatIconUpdate(affectedLogic);
        //combatant.DamageResolutionHandler(outgoingDamage, wasAttack);
    }
}
