using UnityEngine;

public class Pantallas : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator animatorPantalla1;
    public void PantallaSig()
    {
        if(animatorPantalla1.GetInteger("Pantalla") == 0)
        animatorPantalla1.SetInteger("Pantalla", 1);
    }

    public void PantallaAnt()
    {
        if(animatorPantalla1.GetInteger("Pantalla") == 1)
        animatorPantalla1.SetInteger("Pantalla", 0);
    }
}
