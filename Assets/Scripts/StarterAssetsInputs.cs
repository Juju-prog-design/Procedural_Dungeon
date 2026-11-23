using UnityEngine;
using UnityEngine.InputSystem;


public class StarterAssetsInputs : MonoBehaviour
{
    [Header("Valeurs de déplacement du perso")]
    public Vector2 bouge;
    public Vector2 regarder;

    [Header("Commandes du perso")]
    public bool saute;
    public bool cours;
    public bool attaque;
    public bool superAttaque;
    public bool bloquer;


    // Déplacement (WASD)
    public void OnBouge(InputValue value)
    {
        bouge = value.Get<Vector2>();
    }

    // Caméra (Souris)
    public void OnRegarder(InputValue value)
    {
        regarder = value.Get<Vector2>();
    }

    // Saut (Space)
    public void OnSaute(InputValue value)
    {
        saute = value.isPressed;
    }

    // Course (Left Shift)
    public void OnCours(InputValue value)
    {
        cours = value.isPressed;
    }

    // Attaque normale (Left Click)
    public void OnAttaque(InputValue value)
    {
        attaque = value.isPressed;
        Debug.Log("Attaque : " + attaque);
    }

    // Super attaque (E)
    public void OnSuperAttaque(InputValue value)
    {
        superAttaque = value.isPressed;
        Debug.Log("Super Attaque : " + superAttaque);
    }

    // Block avec le bouclier (Right Click)
    public void OnBloquer(InputValue value)
    {
        bloquer = value.isPressed;
        Debug.Log("Bloquer : " + bloquer);
    }
}