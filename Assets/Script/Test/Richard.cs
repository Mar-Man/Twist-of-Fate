﻿using UnityEngine;
using System.Collections;

public class Richard : MonoBehaviour
{
    public static Transform _Transform;

    public static PhysicsManager physManager;
    public float DiedTimer = 2.0f;
    public GameObject Weapon;
	public GameObject Bullet;
    float diedTimer = 0.0f;

    public bool WeaponVisible
    {
        get { if (Weapon != null) return Weapon.gameObject.GetComponent<SpriteRenderer>().enabled; else return false; }
        set
        {
            if (Weapon != null)
            {
                Weapon.gameObject.GetComponent<SpriteRenderer>().enabled = value;
//                Weapon.gameObject.GetComponent<BoxCollider2D>().enabled = value;
            }
        }
    }

    StateManager.State PreviousState
    {
        get { return physManager.State; }
    }

    #region PARTE DEDICATA ALLO SCATTO
    /// <summary>
    /// Tempo minimo necessario per attivare lo scatto alla doppia pressione
    /// della freccia direzionale
    /// </summary>
    public float TimerScatto = 0.5f;
    /// <summary>
    /// Timer usato per la freccia direzionale sinistra
    /// </summary>
    float timerScattoRimastoLeft = 0f;
    /// <summary>
    /// Timer usato per la freccia direzionale destra
    /// </summary>
    float timerScattoRimastoRight = 0f;
    /// <summary>
    /// Controllo per la freccia direzionale
    /// </summary>
    bool tastoDirezionalePrecedentementePremuto = false;
    /// <summary>
    /// Controlla se lo scatto è stato attivato.
    /// Questa variabile è necessaria per dire che lo scatto esiste anche se
    /// il timer scatto ha raggiunto il suo valore di non-scatto.
    /// </summary>
    bool ScattoAttivato = false;
    #endregion

    #region CONTROLLO TASTI PREMUTI
    /// <summary>
    /// Indica se il tasto del salto è stato rilasciato o meno
    /// </summary>
    public bool keySalto = false;
    /// <summary>
    /// Indica se il tasto della scivolata è stato rilasciato o meno
    /// </summary>
    public bool keyScivolata = false;

	#endregion
	/// <summary>
	/// Controlli animazioni
	/// </summary>
    public bool cantMove;
    public bool movimento;

	/// <summary>
	/// Controlli nascondigli
	/// </summary>
	public static bool canHide = false;
	public static bool visto = false;
    public bool Hide
    {
        get { return physManager.Hide; }
        set { physManager.Hide = value; }
    }

    float colorHide = 1.0f;
    bool died = false;

    /// <summary>
    /// Inizializzazione
    /// </summary>
    void Start()
    {
        physManager = GetComponent<PhysicsManager>();
        WeaponVisible = false;
        physManager.hud.GetComponent<HudHandler>().ValueMunitions = 3;
    }

    /// <summary>
    /// Chiamato ad ogni frame
    /// </summary>
    void FixedUpdate()
    {
        // Mi memorizzo lo stato precedente per fare delle comparazioni
        StateManager.State state = PreviousState;
        switch (state)
        {
            case StateManager.State.Jumping:
            case StateManager.State.Falling:
            case StateManager.State.Scivolata:
                // Se in scivolata, disabilita ogni altro cambio di stato:
                // il motore della fisica funziona che, una volta che il
                // personaggio è in scivolata, ci resta finché l'inerzia
                // fa fermare la forza inizialmente assegnata dalla scivolata:
                // di conseguenza disabilita ogni altro tasto di input. Questo
                // per offrire un realismo maggiore.
                break;
            default:
                // Ottiene lo stato a partire dai tasti premuti
                state = getStateFromInput();

                // Controllo che evita di far saltare il personaggio mentre è abbassato
                if (state == StateManager.State.Hide && canHide)
                {
                    colorHide -= Time.deltaTime * 2;
                    if (colorHide <= 0.25f)
                        colorHide = 0.25f;
				if(!visto)
                    Hide = true;
                }
                else if (state == StateManager.State.Hide)
                {
                    state = PreviousState;
                    Hide = false;
                }
                else if (state == StateManager.State.Attack)
                {
                    WeaponVisible = true;
                }
                else
                {
                    WeaponVisible = false;
                    colorHide += Time.deltaTime * 2;
                    if (colorHide >= 1.0f)
                        colorHide = 1.0f;
                }
                GetComponent<SpriteRenderer>().color = new Color(colorHide, colorHide, colorHide);

                if (PreviousState == StateManager.State.Crouch && state == StateManager.State.Jumping)
                {
                    state = PreviousState;
                }
                // Controllo che permette di fare la scivolata
                if (PreviousState == StateManager.State.Crouch && (We.Input.MoveLeft || We.Input.MoveRight) && !keyScivolata)
                {
                    state = StateManager.State.PreScivolata;
                    if (We.Input.MoveLeft)
                        physManager.Direction = false;
                    else
                        physManager.Direction = true;
                    keyScivolata = true;
                }
                break;
        }

        // controllo se il tasto della scivolata è stato rilasciato
        if (!We.Input.MoveLeft && !We.Input.MoveRight)
            keyScivolata = false;

        if (physManager.Health <= 0)
        {
            if (died == false)
            {
                GetComponent<AudioSource>().clip = physManager.SFX[0];
                GetComponent<AudioSource>().Play();
                died = true;
                physManager.speed.y /= 1.5f;
                physManager.speed.x /= 2;
                state = StateManager.State.Died;
            }
            diedTimer += Time.deltaTime;
            if (diedTimer >= DiedTimer)
                Application.LoadLevel(3);

        }
        physManager.State = state;
        _Transform = transform;
    }

    StateManager.State getStateFromInput()
    {
        // controllo se il tasto del salto è stato rilasciato
        if (!We.Input.Jump)
        {
            keySalto = false;
        }

        // TimerScatto serve per far scattare il personaggio alla doppia
        // pressione (velocememnte) di un tasto direzionale. Avremo due
        // contatori (uno per la freccia direzionale destra ed uno per la
        // sinistra) che aumenteranno all'infinito. Più avanti è spiegato
        // il perché viene fatta questa procedura.
        timerScattoRimastoLeft += Time.deltaTime;
        timerScattoRimastoRight += Time.deltaTime;

        // Il salto è posizionato qui sopra perché il giocatore deve permettere al
        // personaggio di farlo saltare sia in camminata sia in corsa. Inoltre, se
        // lo scatto è attivato ed il personaggio salta in corsa, all'atterraggio ha
        // bisogno di continuare la corsa. Sarebbe stato stupido il contrario, indi.
		if (We.Input.Jump == true && !keySalto && physManager.Stamina >= physManager.ConsumoStaminaSalto && !physManager.CheckStairs())
        {
            keySalto = true;
            return StateManager.State.Jumping;
        }
        if (We.Input.MoveLeft == true && !cantMove)
        {
            // Cambia direzione del personaggio in base al tasto direzionale premuto
            physManager.Direction = false;

            // Se il tasto corrente (che è stato appena premuto) non è stato premuto nel frame precedente
            // ed il tempo che è passato dalla prima volta che è stato premuto è minore del tempo richiesto
            // della doppia pressione per attivare lo scatto, allora vai e permetti lo scatto!
            if (((tastoDirezionalePrecedentementePremuto == false && timerScattoRimastoLeft < TimerScatto)
                // Ritorna Run invece che Walk anche se lo scatto è comunque attivo.
			     || ScattoAttivato == true) && physManager.Stamina > 1)
            {
                // Dice che è in scatto.
                ScattoAttivato = true;
                return StateManager.State.Run;
            }
            else
            {
                // Vede se il tasto direzionale è stato precedentemente premuto
                if (tastoDirezionalePrecedentementePremuto == false)
                {
                    // Se no, ora dice il contrario
                    tastoDirezionalePrecedentementePremuto = true;
                    // Ma se ne approfitta per resettare il tempo utile per lo scatto
                    timerScattoRimastoLeft = 0;
                }
                return StateManager.State.Walk;
            }
        }
        if (We.Input.MoveRight == true && !cantMove)
        {
            // Cambia direzione del personaggio in base al tasto direzionale premuto
            physManager.Direction = true;

            if (((tastoDirezionalePrecedentementePremuto == false && timerScattoRimastoRight < TimerScatto)
			     || ScattoAttivato == true) && physManager.Stamina > 1)
            {
                ScattoAttivato = true;
                return StateManager.State.Run;
            }
            else
            {
                if (tastoDirezionalePrecedentementePremuto == false)
                {
                    tastoDirezionalePrecedentementePremuto = true;
                    timerScattoRimastoRight = 0;
                }
                return StateManager.State.Walk;
            }
        }
        // Qui dice che non è stato premuto alcun tasto direzionale
        tastoDirezionalePrecedentementePremuto = false;
        // e che lo scatto per la corsa è disattivato
        ScattoAttivato = false;

        if (We.Input.MoveDown == true && !movimento)
        {
            // Abbassa il personaggio
            transform.position = new Vector2(transform.position.x, transform.position.y - 0.10f);

            return StateManager.State.Crouch;
        }

        if (We.Input.MoveUp == true && !movimento) {
						return StateManager.State.Hide;
				} else
						Hide = false;
						
        if (We.Input.AttackPrimary && !movimento)
            return StateManager.State.Attack;

        if (We.Input.AttackSecondary && !movimento && physManager.hud.GetComponent<HudHandler>().ValueMunitions > 0)
        {
			GameObject istance = (GameObject)Instantiate(Bullet, new Vector3(this.transform.position.x+ (physManager.Direction? + 0.4f:-0.4f), this.transform.position.y +0.25f, this.transform.position.z), transform.rotation);
            MuoviProiettile bullet = istance.GetComponent<MuoviProiettile>();
            bullet.Velocità = physManager.Direction ? +3 : -3;
            physManager.hud.GetComponent<HudHandler>().ValueMunitions--;
            return StateManager.State.Attack2;
        }
		if (We.Input.Defense && !movimento && physManager.Stamina >= physManager.ConsumoStaminaDifesa)
			return StateManager.State.Defense;

        return StateManager.State.Unpressed;
    }

}
