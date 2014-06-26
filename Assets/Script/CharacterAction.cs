﻿using UnityEngine;
using System.Collections;

public class CharacterAction : MonoBehaviour {
	//Tipo enumerativo dei possibili stati del personaggio
	public enum StatoInput{
		Base,
		CamminaDx,
		CamminaSx,
		CorriDx,
		CorriSx,
		Salta,
		Abbassato,
		Muro,
		Arrampicata,
		Attacco,
		Difesa

	}

	//Variabile che contiene lo stato dell'input attuale
	public StatoInput statoCorrente = StatoInput.Base;

	//Tipo enumerativo delledirezioni che può assumere il personaggio
	[HideInInspector] public enum Direzione { DX, Sx }
	[HideInInspector] public Direzione direzionePg;

	//Variabile per la trasformazione grafica della sprite del personaggio
	static public Transform trans;
	//Variabile rigidbody delle proprietà fisiche del personaggio
	protected Rigidbody2D rgbody;

	//Variabili di gestione della fisica del personaggio, possono essere cambiate per cambiare le caratteristiche del movimento
	private float camminata = 1f;
	private float corsa = 2.2f;
	private float forzaSalto = 150f;
	private float gravità = 200f;

	//Controlla se il personaggio si sta muovendo
	public bool movimento = false;
	//Controlla se il personaggio è abbassato
	public bool abbassato = false;
	//Controlla se il personaggio è abbassato
	public bool attacco = false;
	//Controlla se il personaggio è in prossimità delle scale
	public static bool scale = false;
	//Controlla se il giocatore ha premuto il tasto per salire le scale
	public static bool tastoScale = false;
	//Controlla se il giocatore ha premuto il tasto per scendere le scale
	public static bool scendiScale = false;

	//Fixa un bug relativo al salto
	public bool st = true;

	//Variabili che consentono di realizzare il salto pesato
	private float maxTempoSalto = 0.10f;
	public float tempoSalto = 0f;

	//Variabile contenente la velocità ATTUALE assunta dal personaggio
	public Vector2 fisVel = new Vector2();
	//Controlla se il personaggio collide o meno con il terreno
	public bool terra = false;
	//Controlla se il personaggio ha saltato o meno
	public bool salto = false;
	private int groundMask = 1 << 8 | 1 << 10; // Ground layer mask
	private int stairMask = 1 << 10 | 1 << 11; // Stairs layer mask
	private float lung = 0.48f; //Lunghezza raycast 

	//Direzione raycast del controllo dlle scale
	private Vector3 direction;

	//Animatore
	private CharacterAnimation anim;

	public virtual void Awake()
	{
		trans = transform;
		rgbody = rigidbody2D;
		anim = this.GetComponent<CharacterAnimation>();
	}

	// Use this for initialization
	void Start () {

	}

	// FixedUpdate is called once per frame
	void FixedUpdate () {
		setStato ();
		if (statoCorrente == StatoInput.Base) {
			//è fermo, annulla tutte le forze
			anim.setAnimation(StatoInput.Base);
			fisVel = Vector2.zero;
		}

		if (statoCorrente == StatoInput.CamminaDx) {
			//richiama l'animazione della camminata
			anim.setAnimation(StatoInput.CamminaDx);
			
			//modifica la velocità di spostamento
			fisVel.x=camminata;

		}

		if (statoCorrente == StatoInput.CamminaSx) {
			//richiama l'animazione della camminata
			anim.setAnimation(StatoInput.CamminaSx);
			//modifica la velocità di spostamento
			fisVel.x=-camminata;
	
		}
		if (statoCorrente == StatoInput.CorriDx) {
			//richiama l'animazione della corsa
			anim.setAnimation(StatoInput.CorriDx);
			//modifica la velocità di spostamento
			fisVel.x=corsa;
		
		}
		
		if (statoCorrente == StatoInput.CorriSx) {
			//richiama l'animazione della corsa
			anim.setAnimation(StatoInput.CorriSx);
			//modifica la velocità di spostamento
			fisVel.x=-corsa;

		}

		if (statoCorrente == StatoInput.Salta) {
			if(!salto){
				anim.setAnimation(StatoInput.Salta);
				tempoSalto += Time.deltaTime;

				if(tempoSalto<maxTempoSalto)
					fisVel.y=tempoSalto*forzaSalto;
				else{
				//Se il giocatore ha tenuto premuto il pulsante di salto per un certo periodo di tempo, il personaggio scende
				salto = true;
					fisVel.y = 0f;
					st = false;		
				}

			}
		}

		if (statoCorrente == StatoInput.Abbassato) {
			//richiama l'animazione del personaggio che si abbassa

			anim.setAnimation(StatoInput.Abbassato);

		}

		if (statoCorrente == StatoInput.Muro) {
			
		}

		if (statoCorrente == StatoInput.Arrampicata) {
			
		}

		if (statoCorrente == StatoInput.Attacco) {
			//richiama l'animazione dell'attacco

			anim.setAnimation(StatoInput.Attacco);
		}

		if (statoCorrente == StatoInput.Difesa) {
			
		}

		//Gestione Collisione scale
		if (trans.rotation.y == 1)
			direction = Vector3.left; 
				else
			direction = Vector3.right;
		if (Physics2D.Raycast (new Vector2 (trans.position.x, trans.position.y -0.4f), direction, 1.85f, stairMask) || Physics2D.Raycast (new Vector2 (trans.position.x, trans.position.y + 1f), direction, 1.85f, stairMask) || Physics2D.Raycast (new Vector2 (trans.position.x, trans.position.y), Vector3.down, 1.4f, stairMask) ) {
						scale = true;			            
				} else {
						scale = false;
			           			           
				}

		//Debug raycast collisioni scale
		Color cul = Color.green;
		if (scale)
			cul = Color.red;
		Debug.DrawRay (new Vector2 (trans.position.x, trans.position.y-0.4f), direction* 1.85f, cul);
		Debug.DrawRay (new Vector2 (trans.position.x, trans.position.y +1f), direction * 1.85f, cul);
		Debug.DrawRay (new Vector2 (trans.position.x, trans.position.y), Vector3.down * 1.4f, cul);
		//Gestione gravità e terreno///////////////////////////////////////////////////////////////////////////////
		if (Physics2D.Raycast(new Vector2(trans.position.x-0.05f,trans.position.y), Vector3.down, lung, groundMask)||Physics2D.Raycast(new Vector2(trans.position.x+0.05f,trans.position.y), Vector3.down, lung, groundMask))
		{
			terra = true;
			tempoSalto = 0;

			//Il personaggio può abbassarsi solo quando tocca terra
			if(statoCorrente==StatoInput.Abbassato)
			anim.abbassato = true;
		}
		else
		{
			terra = false;
			anim.abbassato = false;
			rgbody.AddForce(Vector3.down * gravità);
		}

		//Debug raycast collisioni terreno
		Color col = Color.green;
		if (terra)
		col = Color.red;
		Debug.DrawRay (new Vector2(trans.position.x-0.1f,trans.position.y), Vector3.down*lung,col);
		Debug.DrawRay (new Vector2(trans.position.x+0.1f,trans.position.y), Vector3.down*lung,col);
		////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //Controllo sull'input del salto, se il giocatore tiene premuto il personaggio non continua a saltare
		//Le variabili che permettono al pesonaggio di saltare vengono riattivate solo quando tocca terra e viene rilasciato il pulsante
		if (!(We.Input.Jump) && terra == true) {
						salto = false;
						st = true;
				}
		//Controllo sulla lunghezza del raycast quando il personaggio è abassato
		if (anim.abbassato == true)
			lung = 0.30f;
		else
			lung = 0.48f;

		//Applica le forze al personaggio solo se non è abbassato
		if(!abbassato)
		rgbody.velocity = new Vector2(fisVel.x, fisVel.y);

	}
	//Gestione Input////
	public void setStato(){

		if (!We.Input.Attack2)
			attacco = false;

		//Salto
		if (We.Input.Jump && statoCorrente != StatoInput.Abbassato && st)
			statoCorrente = StatoInput.Salta;
		else {
			if(!terra){
				salto = true;
				fisVel.y = 0f;
			}		
		
	    //Salire le scale
		if (((We.Input.MoveRight == true)||(We.Input.MoveLeft == true)) && (We.Input.MoveUp == true) && !abbassato )
			tastoScale = true;
			else if (!scale)
			tastoScale = false;
		//Scendere le scale
		if (((We.Input.MoveRight == true)||(We.Input.MoveLeft == true)) && (We.Input.MoveDown == true) && !abbassato  ){
				scendiScale = true;
			    tastoScale = true;
			}
			else
				scendiScale = false;
        
		
		//Attacco
			if(We.Input.Attack2 && !movimento && !attacco){
				statoCorrente = StatoInput.Attacco;
				attacco = true;
			}
		//Camminata e corsa
		else if ((We.Input.MoveRight == true) && (Input.GetKey(KeyCode.Z)) && !abbassato )
						statoCorrente = StatoInput.CorriDx;
						
		else if ((We.Input.MoveLeft == true) && (Input.GetKey(KeyCode.Z)) && !abbassato ) 
						statoCorrente = StatoInput.CorriSx;

		else if (We.Input.MoveLeft == true &&  !abbassato ) 	
						statoCorrente = StatoInput.CamminaSx;

		else if (We.Input.MoveRight == true && !abbassato ) 
				        statoCorrente = StatoInput.CamminaDx;

		//Abbassato
		else if (We.Input.MoveDown == true && !movimento  )	
			statoCorrente = StatoInput.Abbassato;
		else
			statoCorrente = StatoInput.Base;		

		}

	}
	////
}
