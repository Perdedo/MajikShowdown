using UnityEngine;

public class PlayerAnim : MonoBehaviour
{
    private Animator animator;
    //o pInput é pra guardar os inputs de teclado do jogador variando entre -1 a 1 em x e y.
    private Vector2 pInputs;

    //Isso vai definir as variáveis em valor Hash para que o sistema não precise pesquisar através de uma string
     private int InputXHash = Animator.StringToHash("inputX");
     private int InputYHash = Animator.StringToHash("inputy");
     void Awake() 
     {
        animator = GetComponent<Animator>();
     }

    // Update is called once per frame
    void Update()
    {
        //Então o new Vector2 atualiza os valores a cada frame, repassando para as variáveis no animator.
        pInputs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        animator.SetFloat(InputXHash, pInputs.x);
        animator.SetFloat(InputYHash, pInputs.y);
    }
}
