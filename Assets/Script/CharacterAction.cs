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
	public EdgeCollider2D Scala;

	//Tipo enumerativo delledirezioni che può assumere il personaggio
	[HideInInspector] public enum Direzione { DX, Sx }
	[HideInInspector] public Direzione direzionePg;

	//Variabile per la trasformazione grafica della sprite del personaggio
	protected Transform trans;
	//Variabile rigidbody delle proprietà fisiche del personaggio
	protected Rigidbody2D rgbody;

	//Variabili di gestione della fisica del personaggio, possono essere cambiate per cambiare le caratteristiche del movimento
	private float camminata = 1f;
	private float corsa = 2.2f;
	private float forzaSalto = 150f;
	private float gravità = 200f;

	public bool movimento = false;
	public bool abbassato = false;
	public bool scale = false;

	private float maxTempoSalto = 0.10f;
	public float tempoSalto = 0f;

	//Variabile contenente la velocità ATTUALE assunta dal personaggio
	public Vector2 fisVel = new Vector2();
	//controlla se il personaggio collide o meno con il terreno
	public bool terra = false;
	public bool salto = false;
	private int groundMask = 1 << 8; // Ground layer mask
	private int stairMask = 1 << 10; // Stairs layer mask
	private float lung = 0.48f; //Lunghezza raycast 

	//Animatore
	private CharacterAnimation anim;

	public virtual void Awake()
	{
		trans = transform;
		rgbody = rigidbody2D;
		anim = this.GetComponent<CharacterAnimation>();
		Scala = GameObject.FindGameObjectWithTag("Stairs").GetComponent<EdgeCollider2D>();
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
				tempoSalto += Time.deltaTime;

				if(tempoSalto<maxTempoSalto)
					fisVel.y=tempoSalto*forzaSalto;
				else{
				salto = true;
					fisVel.y = 0f;
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
			
		}

		if (statoCorrente == StatoInput.Difesa) {
			
		}

		//Gestione Collisione scale

		if (Physics2D.Raycast (new Vector2 (trans.position.x, trans.position.y), -Vector3.left, 1, stairMask)) {
						scale = true;
				} else {
						scale = false;
			            Scala.enabled = false;
				}
		Color cul = Color.green;
		if (scale)
			cul = Color.red;
		Debug.DrawRay (new Vector2 (trans.position.x, trans.position.y), -Vector3.left* 1, cul);

		//Gestione gravità e terreno///////////////////////////////////////////////////////////////////////////////
		if (Physics2D.Raycast(new Vector2(trans.position.x-0.1f,trans.position.y), Vector3.down, lung, groundMask)||Physics2D.Raycast(new Vector2(trans.position.x+0.1f,trans.position.y), Vector3.down, lung, groundMask))
		{
			terra = true;
			tempoSalto = 0;
			if(statoCorrente==StatoInput.Abbassato)
			anim.abbassato = true;
		}
		else
		{
			terra = false;
			anim.abbassato = false;
			rgbody.AddForce(Vector3.down * gravità);
		}
		Color col = Color.green;
		if (terra)
		col = Color.red;
		Debug.DrawRay (new Vector2(trans.position.x-0.1f,trans.position.y), Vector3.down*lung,col);
		Debug.DrawRay (new Vector2(trans.position.x+0.1f,trans.position.y), Vector3.down*lung,col);
		////////////////////////////////////////////////////////////////////////////////////////////////////////////
        rgbody.velocity = new Vector2(fisVel.x, fisVel.y);

		if (!(We.Input.Jump) && terra == true)
			salto = false;

		if (anim.abbassato == true)
			lung = 0.30f;
		else
			lung = 0.48f;

	}
	//Gestione Input////
	public void setStato(){
        
		if ((We.Input.MoveRight == true) && (We.Input.Attack == true) && abbassato == false)
						statoCorrente = StatoInput.CorriDx;
						
		else if ((We.Input.MoveLeft == true) && (We.Input.Attack == true) && abbassato == false) 
						statoCorrente = StatoInput.CorriSx;
						
		else if (We.Input.MoveRight == true && abbassato == false) 
						statoCorrente = StatoInput.CamminaDx;	

		else if (We.Input.MoveLeft == true &&  abbassato == false) 	
						statoCorrente = StatoInput.CamminaSx;				

		else if (We.Input.MoveDown == true && movimento == false )	
			statoCorrente = StatoInput.Abbassato;
			
		else
			statoCorrente = StatoInput.Base;

		if ((We.Input.MoveRight == true) && (We.Input.MoveUp == true) && abbassato == false && scale )
						Scala.enabled = true;

		
		if (We.Input.Jump && statoCorrente != StatoInput.Abbassato)
						statoCorrente = StatoInput.Salta;
				else {
		         	if(!terra){
						salto = true;
			            fisVel.y = 0f;
	                     		}
				}



	}
	////
}
