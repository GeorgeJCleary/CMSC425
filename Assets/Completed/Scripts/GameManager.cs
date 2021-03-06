﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{

		//variable we can edit to change game experience
		private float thinkingTime = 4f;	 					// thinking time between the level start and first move countdown
		private float firstTurnExtraTime = 2f;					// first Turn Extra Time for countdown timer of the first turn

		private float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		private int maxMoves = 6;								//Maximum number of moves
		public float turnDelay = 0.1f;							//Delay between each Player turn.
		private float prepInteval = 6f;							//Movement preparation interval in seconds	
		public int playerFoodPoints = 100;						//Starting value for Player food points.


		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.

		private bool firstTurn = true;							// true if first turn


		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private GameObject playerobject;
		private Image healthIcon;
		private Image manaIcon;
		private Text alertText;
		private Player player;
		// debugging this
		private int level = 1;									//Current level number, expressed in game as "Day 1".

		public List<MovingObject> enemies;							//List of all Enemy units, used to issue them move commands.

		public List<Projectile> bullets;							//List of all projectile units, used to issue them move commands.

		//private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.
		public bool prepPhase = false;

		private GameObject[] moveBar;
		//private GameObject uipanel;
		private int leveltracker = 0;

		private Slider turnTimer;

		
		//Awake is always called before any Start functions
		void Awake()
		{


			doingSetup = true;
			//Check if instance already exists
			if (instance == null)
				
				//if not, set instance to this
				instance = this;
			
			//If instance already exists and it's not this:
			else if (instance != this)
				//Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
				Destroy(gameObject);	


			//Sets this to not be destroyed when reloading scene
			DontDestroyOnLoad(gameObject);


			//Assign enemies to a new List of Enemy objects.
			enemies = new List<MovingObject>();

			//Assign bullets to a new List of Projectile objects.
			bullets = new List<Projectile>();
			
			//Get a component reference to the attached BoardManager script
			boardScript = GetComponent<BoardManager>();



			//Call the InitGame function to initialize the first level 
			InitGame();

			// it is the first turn of this level
			firstTurn = true;

			//Start the phase changing routine
			StartCoroutine("phaseShifter");

		}

		//Handles the changes between the planning and movement phases
		IEnumerator phaseShifter(){
			while (true) {
				//Debug.Log ("checking if doing setup");
				if (doingSetup) {
					
					Debug.Log ("Think about your moves for " + thinkingTime + " seconds while level sets up.");


					yield return new WaitForSeconds (thinkingTime);
				} else {
				
					//give more time for the first turn
					if (firstTurn) {
						firstTurn = false;
						StartCoroutine (startTimer (prepInteval+ firstTurnExtraTime));
						yield return new WaitForSeconds (prepInteval + firstTurnExtraTime +  1);
					} else {
						StartCoroutine (startTimer (prepInteval));
						yield return new WaitForSeconds (prepInteval + 1);
					}
					//Debug.Log ("entering prep phase");


					if (prepPhase) {
						//TODO add timer
						prepPhase = false;
						StartCoroutine ("processMovements");

						yield return new WaitForSeconds (maxMoves + 1);
					}

				}
			}
		}

		//Processes the movements during the movement phase
		IEnumerator processMovements(){
			//Debug.Log ("entering movement phase");
			for (int i = 0; i < maxMoves; i++) {
				//move player
				player.move ();

				Invoke ("startEnemyMoves", 0.2F);
				//StartCoroutine("MoveEnemies");
				yield return new WaitForSeconds (1.0F);
			}

			//cleanProjectiles();
			//Debug.Log ("MOVING IS DONE");
			prepPhase = true;

		}
		private void startEnemyMoves (){
			StartCoroutine("MoveEnemies");
		}


		private void cleanProjectiles (){
			for (int i = 0; i < bullets.Count; i++)
			{
				if (bullets [i].gameObject.activeSelf == false) {
					Destroy (bullets [i].gameObject);
					bullets.Remove (bullets [i]);
				}

			}
		}

		IEnumerator startTimer(float secondsLeft) {
			//Debug.Log("Countdown started for " + secondsLeft + " seconds.");


			turnTimer.gameObject.SetActive (true);

			turnTimer.maxValue = secondsLeft;

			turnTimer.value = secondsLeft;

			while (secondsLeft > 0f) {
				secondsLeft -= Time.deltaTime;

				turnTimer.value = Mathf.MoveTowards(turnTimer.value, secondsLeft, 0.3F);

				yield return null;
			}

			//hide turn timer when not in use
			turnTimer.gameObject.SetActive (false);

			//Debug.Log("Countdown finished!");
		}
			
		public int getMaxMoves(){
			return maxMoves;
		}

		//This is called each time a scene is loaded.
		void OnLevelWasLoaded(int index)
		{
			//Add one to our level number.
			level++;
			//Call InitGame to initialize our level.
			InitGame();
		}

		public void setup(){
			doingSetup = true;
		}
		
		//Initializes the game for each level.
		void InitGame()
		{
			//While doingSetup is true the player can't move, prevent player from moving while title card is up.
			//doingSetup = true;

			playerobject = GameObject.Find("Player");
			player = playerobject.GetComponent<Player> ();

			manaIcon = GameObject.Find ("ManaImage").GetComponent<Image> ();
			healthIcon = GameObject.Find ("HealthImage").GetComponent<Image> ();

			turnTimer =  GameObject.Find ("TurnTimer").GetComponent<Slider>();

			alertText = GameObject.Find ("AlertText").GetComponent<Text> ();

			hideUI ();

			//Get a reference to our image LevelImage by finding it by name.
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "Level " + level;
			
			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);
			
			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);
			
			//Clear any Enemy objects in our List to prepare for next level.
			enemies.Clear();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardScript.SetupScene(level);



		}

		public bool anyActiveEnemies ()
		{
			for (int i = 0; i < enemies.Count; i++) {

				if (enemies [i].isActiveAndEnabled) {
					return true;
				}
			}
			return false;
		}
		
		//Hides black image used between levels
		void HideLevelImage()
		{
			//Debug.Log("hiding level image!");
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
			
			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
			prepPhase = true;

		}
			
		//Update is called every frame.
		void Update()
		{
			if (!player.paused && Input.GetKeyDown (player.pauseKey)) {

				player.pauseGame ();

			}
			//set gameobjects in the ui to visible after setup is done for the first time for the level
			if (leveltracker != level) {
				
				if (!doingSetup) {
					showUI ();
				}
			}

			if (player.onExit) {
				// stop things for next level

				//if (enemiesDead())
				levelOver ();
			}
		}

		private void showUI(){
			Debug.Log ("showing ui   timer health and move icons");

			//alert text
			alertText.enabled = true;

			//health
			healthIcon.enabled = true;
			healthIcon.GetComponentInChildren<Text>().enabled = true;
			healthIcon.GetComponentsInChildren<Image> ()[1].enabled = true;


			//mana
			manaIcon.enabled = true;
			manaIcon.GetComponentsInChildren<Image> ()[1].enabled = true;

			//turn timer
			turnTimer.gameObject.SetActive (true); 
			turnTimer.value = turnTimer.maxValue;

			//level tracker
			leveltracker = level;


			//move icons
			moveBar = GameObject.FindGameObjectsWithTag ("MoveIcon");
			for (int i = 0; i < moveBar.Length; i++) {
				moveBar [i].GetComponent<Image> ().enabled = true;
			}
		}

		private void hideUI(){
			Debug.Log ("hiding  ui   timer health and move icons");



			//text
			alertText.enabled = false;
			//health
			healthIcon.enabled = false;
			healthIcon.GetComponentInChildren<Text> ().enabled = false;
			healthIcon.GetComponentsInChildren<Image> ()[1].enabled = false;
			//mana
			manaIcon.enabled = false;
			manaIcon.GetComponentsInChildren<Image> ()[1].enabled = false;
			//turn timer
			turnTimer.gameObject.SetActive (false);

			//move icons
			moveBar = GameObject.FindGameObjectsWithTag ("MoveIcon");
			for (int i = 0; i < moveBar.Length; i++) {
				moveBar [i].GetComponent<Image> ().enabled = false;
			}
		}

		private void levelOver(){
			
			prepPhase = false;
			firstTurn = true;
			//next level
			StopCoroutine ("processMovements");
			StopCoroutine ("MoveEnemies");
			StopCoroutine ("startTimer");
			//StopCoroutine ("phaseShifter");

			//kill all projectiles
			GameObject[] bullet = GameObject.FindGameObjectsWithTag ("Projectile");
			for (int i = 0; i < bullet.Length; i++)
			{
				Projectile p = bullet [i].GetComponent<Projectile> ();
				p.gameObject.SetActive (false);
				Destroy (p.gameObject);
				Destroy (p);

			}

		}
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(MovingObject script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}

		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void removeEnemyfromList(MovingObject script)
		{
			//Add Enemy to List enemies.
			enemies.Remove(script);
		}

		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddBulletToList(Projectile script)
		{
			//Add Enemy to List enemies.
			bullets.Add(script);
		}
			
		
		
		//GameOver is called when the player reaches 0 food points
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			if (level == 1) {
				levelText.text = "You made it " + level + " floor\ndeep before you died.";
			} else {
				levelText.text = "You made it " + level + " floors\ndeep before you died.";
			}
			
			//Enable black background image gameObject.
			levelImage.SetActive(true);

			hideUI ();
			player.showClose ();

			//Disable this GameManager.
			enabled = false;

			StopCoroutine ("processMovements");
			StopCoroutine ("MoveEnemies");
			StopCoroutine ("startTimer");
			StopCoroutine ("phaseShifter");
		}
		
		//Coroutine to move enemies in sequence.
		IEnumerator MoveEnemies()
		{
			//While enemiesMoving is true player is unable to move.
			//enemiesMoving = true;


			// what does this code do?
			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			yield return new WaitForSeconds(turnDelay);
			
			//If there are no enemies spawned (IE in first level):
			//if (enemies.Count == 0) 
			//{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
				//yield return new WaitForSeconds(turnDelay);
			//}

			GameObject[] bullet = GameObject.FindGameObjectsWithTag ("Projectile");
			for (int i = 0; i < bullet.Length; i++)
			{
				Projectile p = bullet [i].GetComponent<Projectile> ();
				//Debug.Log("print bullet array: "+ bullets.);
				if (p != null) {
					float temp = p.moveTime;

					p.MoveBullet ();

					//Wait for Enemy's moveTime before moving next Enemy, 
					yield return new WaitForSeconds (temp);
				}

			}

			
			//Loop through List of Enemy objects.
			for (int i = 0; i < enemies.Count; i++)
			{
				if (enemies [i].isActiveAndEnabled) {
					//Debug.Log("Moving enemy: "+ i);
					//Call the MoveEnemy function of Enemy at index i in the enemies List.
					Enemy e1;
					Enemy2 e2;
					if (enemies [i].GetComponent<Enemy> () != null) {
						e1 = (Enemy)enemies [i];
						e1.MoveEnemy ();
					} else {
						e2 = (Enemy2)enemies [i];
						e2.MoveEnemy ();
					}

				
					//Wait for Enemy's moveTime before moving next Enemy, 
					yield return new WaitForSeconds (enemies [i].moveTime);
				}
			}
			//Once Enemies are done moving, set playersTurn to true so player can move.
			//playersTurn = true;
			
			//Enemies are done moving, set enemiesMoving to false.
			//enemiesMoving = false;

			yield return null;
		}
	}
}

