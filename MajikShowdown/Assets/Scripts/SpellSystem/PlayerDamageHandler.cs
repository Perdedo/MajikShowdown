using UnityEngine;

public class PlayerDamageHandler : CharacterDamageHandler
{
    public override void Die()
    {
        //Atualizar depois com sistema de reviver
        Debug.Log("morri");
    }
}
