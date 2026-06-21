using Mirror;
using System;
using UnityEngine;

public class PlayerDamageHandler : CharacterDamageHandler
{
    public Player myPlayer;
    public override void TakeDamage(Damage damage)
    {
        if (!isServer && network)
        {
            return;
        }
        float finalDamage = damage.Value;
        for (int i = 0; i < Resistances.Count; i++)
        {
            if (Resistances[i].Element == damage.Element)
            {
                finalDamage *= 1 - Resistances[i].PercentValue / 100;
                i = Resistances.Count;
            }
        }
        Health = MathF.Max(Health - finalDamage, 0);
        if (Health <= 0)
        {
            Die();
        }
        UpdateUI();
    }

    [TargetRpc]
    public void UpdateUI()
    {
        GameManager.Instance.uiController.playerUI.UpdateHealthUI();
    }

    public override void Heal(float amount)
    {
        if (!isServer && network)
        {
            return;
        }
        Health = Mathf.Min(Health + amount, MaxHealth);
        UpdateUI();
    }
    public override void Die()
    {
        //Atualizar depois com sistema de reviver
        myPlayer.dead = true;
        FlowFieldManager.instance.UpdateFlowField();
        GameManager.Instance.hordeController.CheckDeadPlayers();
        //Disappear();
        //Debug.Log("morri");
    }

    /*[ClientRpc]
    public void Disappear()
    {
        this.gameObject.SetActive(false);
    }*/


    public void Respawn()
    {
        if(!isServer && network)
        {
            return;
        }
        Health = MaxHealth / 2;
        myPlayer.dead = false;
        FlowFieldManager.instance.UpdateFlowField();
        //Reappear();
    }


    /*[ClientRpc]
    public void Reappear()
    {
        this.gameObject.SetActive(true);
    }*/
}
