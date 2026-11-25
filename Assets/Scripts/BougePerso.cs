using UnityEngine;


public class BougePerso : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Vitesse de marche du perso")]
    [SerializeField] private float vitesseMarche = 6.0f;

    [Tooltip("Vitesse de course du perso")]
    [SerializeField] private float vitesseCourse = 15.0f;

    [Tooltip("Accélération et décélération du perso")]
    [SerializeField] private float accelerationDeceleration = 10.0f;

    [Tooltip("Vitesse de décélération du perso")]
    [SerializeField] private float deceleration = 30.0f;


    [Space(5)]
    [Header("Saut")]
    [Tooltip("Hauteur du saut du perso")]
    [SerializeField] private float hauteurSaut = 1.2f;

    [Tooltip("Valeur de gravité")]
    [SerializeField] private float gravite = -15.0f;

    [Tooltip("Temps nécessaire pour sauter à nouveau")]
    [SerializeField] private float delaiSaut = 0.1f;

    // Délai nécessaire pour sauter à nouveau
    private float sautTimeout;


    [Space(5)]
    [Header("Perso au sol")]
    [Tooltip("Détermine si le perso touche le sol ou non")]
    [SerializeField] private bool toucheSol = true;

    [Tooltip("Distance du perso du sol")]
    [SerializeField] private float distanceDuSol = -0.14f;

    [Tooltip("Radius du Character controller pour savoir si le perso touche au sol ou non")]
    [SerializeField] private float GroundedRadius = 0.5f;

    [Tooltip("Layer du sol")]
    [SerializeField] private LayerMask layerSol;


    [Space(5)]
    [Header("Cinemachine")]
    [SerializeField] private GameObject cameraTP;


    [Space(5)]
    [Header("Animator")]
    [SerializeField] private Animator animator;


    [Space(5)]
    [Header("Vitesse")]
    // Vitesse verticale
    private float vitesseVerticale;

    // Valeur du changement de vitesse du perso
    private float changementVitesse;

    // Vélocité horizontale actuelle du perso
    private Vector3 vitesseHorizontaleActuelle;

    // Vélocité actuelle de rotation du perso
    private float vitesseRotationActuelle;


    //[Space(5)]
    [Header("Pts de vie")]
    [Tooltip("Durée d'invincibilité après avoir reçu des dégâts")]
    [SerializeField] private float dureeInvincibilite = 1.5f;

    // Temps où le perso pourra reprendre des dégâts
    private float prochainDegatPossible = 0f;


    //[Space(5)]
    //[Header("Sons")]
    // GameObject avec l'effet sonore du perso
    //[SerializeField] private GameObject ouch;

    // AudioSources
    //private AudioSource audioPrincipal;
    //private AudioSource audioOuch;


    [Space(5)]
    [Header("Autres")]
    // Character controller du perso
    private CharacterController controller;

    // Script "StarterAssetsInputs" du perso
    private StarterAssetsInputs lesInputs;

    // SO "InfosPerso" du perso
    [SerializeField] private InfosPerso infosPerso;

    // Indique si la défaite a déjà été programmée
    private bool defaiteEnAttente = false;

    // Indique si la partie est terminée
    private bool partieTerminee = false;


    // ---------------------------------------------------------------------------------------------------------------------------------------------------

    // Est appelée au lancement de la scène
    void Start()
    {
        // Verrouille le curseur
        //Cursor.lockState = CursorLockMode.Confined;

        // Met à jour l'affichage initial des PV
        infosPerso.nbPV = 4;
        PointsVie();

        // Trouve les composantes du Character controller et le script "StarterAssetsInputs" du perso
        controller = GetComponent<CharacterController>();
        lesInputs = GetComponent<StarterAssetsInputs>();

        // Réinitialise les délais de saut
        sautTimeout = delaiSaut;

        // Récupère l'AudioSource de la caméra principale et du perso
        //audioOuch = ouch.GetComponent<AudioSource>();
        //GameObject cameraPrincipale = GameObject.Find("Main Camera");
        //audioPrincipal = cameraPrincipale.GetComponent<AudioSource>();

        // Affiche le panneau du début
        //panneauDebut.SetActive(true);

        // Invoque la méthode "EnleverPanneaux" dans 3 secondes
        //Invoke(nameof(EnleverPanneaux), 3f);
    }


    // Est appelée à chaque frame
    void Update()
    {
        // Arrête l'exécution si la partie est terminée
        if (partieTerminee)
            return;

        // Exécute les méthodes suivantes à chaque frame
        VerifierSol();
        Bouge();
        Attaque();
        Bloquer();
        SautEtGravite();
    }


    // -----------------------------------------------------------------------CINEMACHINES + BOUGE------------------------------------------------------------------

    // Déplacement du perso en 3e personne
    private void Bouge()
    {
        // Récupère les entrées du perso
        Vector2 direction = lesInputs.bouge;

        // Détermine la vitesse souhaitée
        float vitesseVoulue = lesInputs.cours ? vitesseCourse : vitesseMarche;

        // Récupère l'orientation horizontale de la caméra
        float angleCamera = cameraTP.transform.eulerAngles.y;

        // Calcule la direction du déplacement selon la caméra
        Vector3 directionDeplacement = Quaternion.Euler(0f, angleCamera, 0f) * new Vector3(direction.x, 0f, direction.y);

        // Normalise la direction pour éviter une vitesse diagonale supérieure
        directionDeplacement.Normalize();

        // Détermine la rotation souhaitée du perso
        if (directionDeplacement.sqrMagnitude > 0.01f)
        {
            float angleCible = Mathf.Atan2(directionDeplacement.x, directionDeplacement.z) * Mathf.Rad2Deg;
            float angleActuel = Mathf.SmoothDampAngle(transform.eulerAngles.y, angleCible, ref vitesseRotationActuelle, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angleActuel, 0f);
        }

        // Déplace le perso
        Deplacer(directionDeplacement, vitesseVoulue);
    }


    // Accélération, décélération et déplacement du perso
    void Deplacer(Vector3 direction, float vitesseVoulue)
    {
        // Détermine la vitesse horizontale souhaitée
        Vector3 vitesseVoulueHorizontale = direction * vitesseVoulue;

        // Détermine si le perso change de direction
        bool changeDirection = vitesseVoulueHorizontale.sqrMagnitude > 0.01f &&
            vitesseHorizontaleActuelle.sqrMagnitude > 0.01f && Vector3.Dot(vitesseHorizontaleActuelle.normalized, vitesseVoulueHorizontale.normalized) < 0.95f;

        // Détermine la vitesse de changement
        float vitesseChangement;

        if (vitesseVoulueHorizontale == Vector3.zero || changeDirection)
            vitesseChangement = deceleration;
        else
            vitesseChangement = accelerationDeceleration;

        // Accélère, ralentit ou change la direction du perso
        vitesseHorizontaleActuelle = Vector3.MoveTowards(vitesseHorizontaleActuelle, vitesseVoulueHorizontale, vitesseChangement * Time.deltaTime);

        // Calcule la vitesse actuelle du perso
        float vitesse = vitesseHorizontaleActuelle.magnitude;

        // Détermine le seuil maximal de l'animation selon le déplacement
        float seuilVitesse = Mathf.Approximately(vitesseVoulue, vitesseCourse) ? 6.0f : 2.0f;

        // Calcule la progression de la vitesse entre 0 et 1
        float progressionVitesse = vitesseVoulue > 0.0f ? Mathf.Clamp01(vitesse / vitesseVoulue) : 0.0f;

        // Détermine la valeur à transmettre à l'Animator
        float vitesseAnimation = progressionVitesse * seuilVitesse;

        // Rend plus fluide le changement de vitesse de l'animation
        changementVitesse = Mathf.Lerp(changementVitesse, vitesseAnimation, Time.deltaTime * accelerationDeceleration);

        // Si le changement de vitesse est plus petit que 0.01, met sa valeur à 0
        if (changementVitesse < 0.01f)
            changementVitesse = 0;

        // Donne la valeur du changement de vitesse à l'Animator
        animator.SetFloat("Vitesse", changementVitesse);

        // Déplace le perso avec son Character Controller
        controller.Move(vitesseHorizontaleActuelle * Time.deltaTime + Vector3.up * (vitesseVerticale * Time.deltaTime));
    }


    // ------------------------------------------------------------------------ATTAQUE----------------------------------------------------------------

    // Gestion des attaques du perso
    void Attaque()
    {
        // Vérifie si le perso est en train de bloquer
        bool blocageEnCours = animator.GetBool("IsBlocking");

        // Vérifie si une attaque est en cours
        bool attaqueEnCours = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        // Vérifie aussi la prochaine animation si une transition est en cours
        if (animator.IsInTransition(0))
        {
            attaqueEnCours |= animator.GetNextAnimatorStateInfo(0).IsTag("Attack");
        }

        // Empêche les attaques pendant le blocage, une autre attaque ou lorsque le perso est en l'air
        if (blocageEnCours || attaqueEnCours || !toucheSol)
        {
            lesInputs.attaque = false;
            lesInputs.superAttaque = false;
            return;
        }

        // Attaque normale
        if (lesInputs.attaque)
        {
            animator.SetTrigger("Attack");
            lesInputs.attaque = false;
        }

        // Super attaque
        if (lesInputs.superAttaque)
        {
            animator.SetTrigger("SuperAttack");
            lesInputs.superAttaque = false;
        }
    }


    // ------------------------------------------------------------------------BLOCK----------------------------------------------------------------

    // Gestion du blocage avec le bouclier
    void Bloquer()
    {
        // Vérifie si une attaque est en cours
        bool attaqueEnCours = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");

        // Vérifie aussi la prochaine animation si une transition est en cours
        if (animator.IsInTransition(0))
            attaqueEnCours |= animator.GetNextAnimatorStateInfo(0).IsTag("Attack");

        // Détermine si le perso peut bloquer
        bool peutBloquer = toucheSol && !attaqueEnCours;

        // Active ou désactive le blocage
        animator.SetBool("IsBlocking", lesInputs.bloquer && peutBloquer);
    }


    // ------------------------------------------------------------------------DÉSACTIVATION----------------------------------------------------------------

    // Désactive l'Audio source des pièces et de la douleur du perso
    void DesactiveSons()
    {
        //audioOuch.enabled = false;
    }


    // Enlève les panneaux actifs
    void EnleverPanneaux()
    {
        //panneauDebut.SetActive(false);
        //panneauLimites.SetActive(false);
    }


    // ------------------------------------------------------------------------COLLISIONS-------------------------------------------------------------------

    // Quand le perso entre en collision avec qq
    void OnTriggerEnter(Collider collision)
    {
        // Ignore les collisions si la partie est terminée
        if (partieTerminee)
            return;


        // Murs limites du terrain
        if (collision.CompareTag("mursLimites"))
        {
            // Affiche le panneau
            //panneauLimites.SetActive(true);

            // Invoque la méthode "EnleverPanneaux" dans 2 secondes
            Invoke(nameof(EnleverPanneaux), 2f);
        }


        // Collision avec les ennemis
        if (collision.CompareTag("Ennemi"))
        {
            // Ignore les dégâts pendant le blocage
            if (animator.GetBool("IsBlocking"))
            {
                animator.SetTrigger("BlockDamage");
                return;
            }

            // Ignore les dégâts pendant l'invincibilité
            if (Time.time < prochainDegatPossible)
                return;

            // Définit le moment où le perso pourra subir de nouveaux dégâts
            prochainDegatPossible = Time.time + dureeInvincibilite;

            // Diminue les PV sans passer sous 0
            infosPerso.nbPV = Mathf.Max(0, infosPerso.nbPV - 1);

            // Active le son de douleur
            //audioOuch.enabled = true;

            // Déclenche l'animation de dégâts
            animator.SetTrigger("IsDamaged");

            // Affiche les PV mis à jour
            PointsVie();

            // Désactive les sons temporaires après 1 seconde
            Invoke(nameof(DesactiveSons), 1f);
        }
    }

    // Gestion des PV du perso
    void PointsVie()
    {
        // Empêche d'avoir moins de 0 PV ou plus de 4 PV
        infosPerso.nbPV = Mathf.Clamp(infosPerso.nbPV, 0, 4);

        // Si le perso n'a plus de PV
        if (infosPerso.nbPV == 0 && !defaiteEnAttente)
        {
            // Empêche de terminer la partie plusieurs fois
            defaiteEnAttente = true;
            partieTerminee = true;

            // Déclenche l'animation de mort
            animator.SetBool("IsDead", true);
        }
    }


    // ------------------------------------------------------------------------VICTOIRE + DÉFAITE--------------------------------------------------------

    // Termine la partie et charge la scène correspondante
    //void TerminerPartie(string nomScene)
    //{
    // Empêche de terminer la partie plusieurs fois
    //if (partieTerminee)
    //return;

    //partieTerminee = true;

    // Arrête les musiques
    //audioPrincipal.enabled = false;

    // Charge la scène correspondante
    //SceneManager.LoadScene(nomScene);
    //}


    // Aller à la scène de victoire
    //void SceneVictoire()
    //{
    //TerminerPartie("Victory");
    //}


    // Aller à la scène de défaite
    //void SceneDefaite()
    //{
    //TerminerPartie("Defeat");
    //}


    // ------------------------------------------------------------------------SAUT-------------------------------------------------------------------

    // Détermine si le perso touche au sol ou non (création d'une sphère sous le perso)
    void VerifierSol()
    {
        // Position de la sphère sous le perso
        Vector3 spherePosition = transform.position + Vector3.up * distanceDuSol;

        // Vérifie si la sphère touche le sol
        toucheSol = Physics.CheckSphere(spherePosition, GroundedRadius, layerSol, QueryTriggerInteraction.Ignore);

        // Met à jour l'Animator
        animator.SetBool("IsGrounded", toucheSol);
    }


    // Permet au perso de sauter et applique la gravité
    void SautEtGravite()
    {
        // Vitesse maximale de chute
        float vitesseFinale = -53.0f;

        // Applique la gravité sans dépasser la vitesse maximale de chute
        if (vitesseVerticale > vitesseFinale)
            vitesseVerticale = Mathf.Max(vitesseVerticale + gravite * Time.deltaTime, vitesseFinale);

        // Vérifie si le perso est en train d'attaquer ou de bloquer
        bool attaqueEnCours = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        bool blocageEnCours = animator.GetBool("IsBlocking");

        // Vérifie aussi la prochaine animation si une transition est en cours
        if (animator.IsInTransition(0))
            attaqueEnCours |= animator.GetNextAnimatorStateInfo(0).IsTag("Attack");

        // Si le perso touche au sol
        if (toucheSol)
        {
            // Empêche la vélocité de baisser à l'infini quand le perso touche au sol
            if (vitesseVerticale < 0.0f)
                vitesseVerticale = -2f;

            // Si on appuie sur la barre d'espace, que le délai est terminé et qu'aucune action ne bloque le saut
            if (lesInputs.saute && sautTimeout <= 0.0f && !attaqueEnCours && !blocageEnCours)
            {
                vitesseVerticale = Mathf.Sqrt(hauteurSaut * -2f * gravite);
                animator.SetBool("IsJumping", true);
                lesInputs.saute = false;
            }

            // Diminue le délai pour sauter à nouveau
            if (sautTimeout > 0.0f)
                sautTimeout -= Time.deltaTime;
        }

        else
        {
            // Réinitialise le délai pour sauter à nouveau
            sautTimeout = delaiSaut;

            // Ignore une nouvelle pression de saut pendant que le perso est en l'air
            lesInputs.saute = false;
        }

        // Termine l'état de saut lorsque le perso retouche le sol
        if (toucheSol && vitesseVerticale <= 0.0f)
            animator.SetBool("IsJumping", false);
    }
}