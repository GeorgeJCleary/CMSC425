﻿using UnityEngine;
using System.Collections;

namespace Completed
{
	using System.Collections.Generic;		//Allows us to use Lists. 
	using UnityEngine.UI;					//Allows us to use UI.
	
	public class GameManager : MonoBehaviour
	{
		private float levelStartDelay = 2f;						//Time to wait before starting level, in seconds.
		private int maxMoves = 7;								//Maximum number of moves
		//public float turnDelay = 0.1f;							//Delay between each Player turn.
		private float prepInteval = 5f;							//Movement preparation interval in seconds
		private float thinkingTime = 2f;	
		public int playerFoodPoints = 100;						//Starting value for Player food points.
		public static GameManager instance = null;				//Static instance of GameManager which allows it to be accessed by any other script.
		[HideInInspector] public bool playersTurn = true;		//Boolean to check if it's players turn, hidden in inspector but public.

		private Text levelText;									//Text to display current level number.
		private GameObject levelImage;							//Image to block out level as levels are being set up, background for levelText.
		private BoardManager boardScript;						//Store a reference to our BoardManager which will set up the level.
		private GameObject playerobject;
		public Player player;
		private int level = 1;									//Current level number, expressed in game as "Day 1".
		private List<Enemy> enemies;							//List of all Enemy units, used to issue them move commands.
		private bool enemiesMoving;								//Boolean to check if enemies are moving.
		private bool doingSetup = true;							//Boolean to check if we're setting up board, prevent Player from moving during setup.
		public bool prepPhase = false;


		public Slider turnTimer;
		private float timer = 0.0F;

		
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


			turnTimer =  GameObject.Find ("TurnTimer").GetComponent<Slider>();
			turnTimer.maxValue = prepInteval;

			//Assign enemies to a new List of Enemy objects.
			enemies = new List<Enemy>();
			
			//Get a component reference to the attached BoardManager script
			boardScript = GetComponent<BoardManager>();



			//Call the InitGame function to initialize the first level 
			InitGame();

			//Start the phase changing routine
			StartCoroutine("phaseShifter");

		}

		//Handles the changes between the planning and movement phases
		IEnumerator phaseShifter(){
			while (true) {
				Debug.Log ("checking if doing setup");
				if (doingSetup) {
					Debug.Log ("Think about your moves for " + thinkingTime + " seconds while level sets up.");
					yield return new WaitForSeconds (thinkingTime);
				} else {

				
					StartCoroutine (startTimer (prepInteval));

					Debug.Log ("entering prep phase");

					yield return new WaitForSeconds (prepInteval + 1);

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
			Debug.Log ("entering movement phase");
			for (int i = 0; i < maxMoves; i++) {
				//move player
				player.move ();


				StartCoroutine("MoveEnemies");
				yield return new WaitForSeconds (1);
			}

			Debug.Log ("MOVING IS DONE");
			prepPhase = true;

		}

		IEnumerator startTimer(float secondsLeft) {
			Debug.Log("Countdown started for " + secondsLeft + " seconds.");
			turnTimer.gameObject.SetActive (true); 
			turnTimer.value = secondsLeft;

			while (secondsLeft > 0f) {
				secondsLeft -= Time.deltaTime;

				turnTimer.value = Mathf.MoveTowards(turnTimer.value, secondsLeft, 0.3F);

				yield return null;
			}

			Debug.Log("Countdown finished!");
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

			turnTimer =  GameObject.Find ("TurnTimer").GetComponent<Slider>();
			turnTimer.maxValue = prepInteval;
			timer = prepInteval;
			turnTimer.gameObject.SetActive (false);

			
			//Get a reference to our image LevelImage by finding it by name.
			levelImage = GameObject.Find("LevelImage");
			
			//Get a reference to our text LevelText's text component by finding it by name and calling GetComponent.
			levelText = GameObject.Find("LevelText").GetComponent<Text>();
			
			//Set the text of levelText to the string "Day" and append the current level number.
			levelText.text = "Day " + level;
			
			//Set levelImage to active blocking player's view of the game board during setup.
			levelImage.SetActive(true);
			
			//Call the HideLevelImage function with a delay in seconds of levelStartDelay.
			Invoke("HideLevelImage", levelStartDelay);
			
			//Clear any Enemy objects in our List to prepare for next level.
			enemies.Clear();
			
			//Call the SetupScene function of the BoardManager script, pass it current level number.
			boardScript.SetupScene(level);



		}
		
		
		//Hides black image used between levels
		void HideLevelImage()
		{
			Debug.Log("hiding level image!");
			//Disable the levelImage gameObject.
			levelImage.SetActive(false);
			
			//Set doingSetup to false allowing player to move again.
			doingSetup = false;
			prepPhase = true;

			StopCoroutine ("processMovements");
			StopCoroutine ("MoveEnemies");
			StopCoroutine ("startTimer");


		}
		
		//Update is called every frame.
		void Update()
		{
		}
		
		//Call this to add the passed in Enemy to the List of Enemy objects.
		public void AddEnemyToList(Enemy script)
		{
			//Add Enemy to List enemies.
			enemies.Add(script);
		}
		
		
		//GameOver is called when the player reaches 0 food points
		public void GameOver()
		{
			//Set levelText to display number of levels passed and game over message
			levelText.text = "After " + level + " days\n,you died.";
			
			//Enable black background image gameObject.
			levelImage.SetActive(true);


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
			enemiesMoving = true;
			
			//Wait for turnDelay seconds, defaults to .1 (100 ms).
			//yield return new WaitForSeconds(turnDelay);
			
			//If there are no enemies spawned (IE in first level):
			//if (enemies.Count == 0) 
			//{
				//Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
				//yield return new WaitForSeconds(turnDelay);
			//}
			
			//Loop through List of Enemy objects.
			for (int i = 0; i < enemies.Count; i++)
			{
				Debug.Log("Moving enemy: "+ i);
				//Call the MoveEnemy function of Enemy at index i in the enemies List.
				enemies[i].MoveEnemy ();
				
				//Wait for Enemy's moveTime before moving next Enemy, 
				yield return new WaitForSeconds(enemies[i].moveTime);

			}
			//Once Enemies are done moving, set playersTurn to true so player can move.
			//playersTurn = true;
			
			//Enemies are done moving, set enemiesMoving to false.
			enemiesMoving = false;

			yield return null;
		}
	}
}

