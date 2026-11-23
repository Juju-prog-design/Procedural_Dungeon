using UnityEngine;


// Permet de créer un SO dans le dossier "SO"
[CreateAssetMenu(fileName = "InfosPerso", menuName = "SO/NouveauPerso")]


public class InfosPerso : ScriptableObject
{
    // Nb d'ennemis tués par le perso
    public int nbEnnemis;

    // Nb de PV du perso
    public int nbPV;
}