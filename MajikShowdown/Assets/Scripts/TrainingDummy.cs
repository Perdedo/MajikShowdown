using UnityEngine;

public class TrainingDummy : MonoBehaviour, IGameCharacter
{
    public CharacterDamageHandler DamageHandler { get; private set; }

    private void Awake()
    {
        DamageHandler = GetComponent<CharacterDamageHandler>();

        if (DamageHandler == null)
        {
            Debug.LogError("[TrainingDummy] CharacterDamageHandler not found.");
            return;
        }

        DamageHandler.gameCharacter = this;
    }

    public void Knockback(Vector3 direction, float strenght)
    {
        // Training Dummy does not receive knockback.
    }

    public void Die()
    {
        // Training Dummy does not die.
    }
}