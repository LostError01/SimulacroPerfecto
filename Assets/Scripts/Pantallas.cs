using UnityEngine;

public class Pantallas : MonoBehaviour
{
    [Header("Animators")]
    [SerializeField] private Animator animatorPantalla1;

    private int pantallaActual = 0;

    private void Update()
    {
        pantallaActual = animatorPantalla1.GetInteger("Pantalla");
    }
    public void PantallaSig()
    {
        if(pantallaActual == 0)
        animatorPantalla1.SetInteger("Pantalla", 1);

        if(pantallaActual == -1)
            animatorPantalla1.SetInteger("Pantalla", 0);
    }

    public void PantallaAnt()
    {
        if (pantallaActual == 1)
        {
            animatorPantalla1.SetInteger("Pantalla", 0);
        }
        
        if (pantallaActual == 0)
        {
            animatorPantalla1.SetInteger("Pantalla", -1);
        }

    }
}
