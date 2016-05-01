using UnityEngine;
using System.Collections;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player: MovingObject
	{
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		public int pointsPerFood = 10;				//Number of points to add to player food points when picking up a food object.
		public int pointsPerSoda = 20;				//Number of points to add to player food points when picking up a soda object.
		public int wallDamage = 1;					//How much damage a player does to a wall when chopping it.
		public Text foodText;						//UI Text to display current player food total.
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 2 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 2 Audio clips to play when player collects a food object.
		public AudioClip drinkSound1;				//1 of 2 Audio clips to play when player collects a soda object.
		public AudioClip drinkSound2;				//2 of 2 Audio clips to play when player collects a soda object.
		public AudioClip gameOverSound;				//Audio clip to play when player dies.

		public Image healthIcon;

		public Image moveSinglePanel;

		public Sprite upArrow;
		public Sprite downArrow;
		public Sprite leftArrow;
		public Sprite rightArrow;
		public Sprite moveBorder;

		public bool onExit = false;

		private int maxMoves;
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int food;							//Used to store player food points total during level.
		private Queue moves;					//Movement Queue used to store movements to executed later

		private Queue moveBarQ;
		private GameObject[] moveBar;

		private bool prepPhase;

		private readonly byte UP = 0;			//Un-edittable bytes indicating direction on stack
		private readonly byte LEFT = 1;			//			0
		private readonly byte DOWN = 2;			//		1		3
		private readonly byte RIGHT = 3;		//			2
		private readonly byte ATTACK = 4;



		
		//Start overrides the Start function of MovingObject
		protected override void Start ()
		{

			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();

			//Get the current food point total stored in GameManager.instance between levels.
			food = GameManager.instance.playerFoodPoints;


			onExit = false;

			//Initiaize Queues
			moves = new Queue();
			moveBarQ = new Queue();

			//Debug.Log ("made Queue");
			//Debug.Log (moves.Count);
			//Get the number of maximum moves a player can queue
			maxMoves = GameManager.instance.getMaxMoves();

			//make ui bar with max move slots
			makeMoveBar(maxMoves);

			moveBar = GameObject.FindGameObjectsWithTag ("MoveIcon");

			//Set the foodText to reflect the current player food total.
			foodText.text = "Health: " + food;
			
			//Call the Start function of the MovingObject base class.
			base.Start ();
		}

		private void makeMoveBar(int moves){
			GameObject panel = GameObject.Find ("Panel");
			for (int i = 0; i < moves; i++) {
				Image m = (Image) Instantiate(moveSinglePanel);

				//m.rectTransform.localPosition = new Vector2(275 + i*32, 12);

				m.rectTransform.SetParent (panel.GetComponent<RectTransform>(),false);

				m.transform.SetParent( panel.transform, false); 

				m.rectTransform.sizeDelta = new Vector2(32, 32);
				//m.rectTransform.anchoredPosition = Vector2.one;
				m.transform.localScale = Vector2.one;
				m.rectTransform.localPosition = new Vector2(100 + i*40, 20);


				//m.rectTransform.sizeDelta = new Vector2(32, 32);

				//m.transform.parent = panel.transform; 

				m.name = "moveIcon" + i;
				m.gameObject.name = "moveIcon" + i;

				m.tag = "MoveIcon";
				m.gameObject.tag = "MoveIcon";



				//change picture to empty pic
				//m.sprite = moveBorder;

				m.enabled = false;

				//Debug.Log("Making movepanel " + i);
			}
		}

		private void setMove(GameObject moveTile, byte dir){
			Image i = moveTile.GetComponent<Image>();
			if (dir == UP) {
				i.sprite = upArrow;
			}else if (dir == LEFT) {
				i.sprite = leftArrow;
			}else if (dir == DOWN){
				i.sprite = downArrow;
			}else if (dir == RIGHT){
				i.sprite = rightArrow;
			}else if (dir == ATTACK){
				i.sprite = null;
			}else{
				Debug.Log("Something other that a direction needs to go in the movetile");
			}
		}

		//returns true if movement queue is empty
		public bool hasMovesLeft(){
			return moves.Count != 0;
		}

		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
			GameManager.instance.playerFoodPoints = food;
		}

		public void move(){
			//Debug.Log ("Entering move with "+moves.Count+" moves left");
			if (hasMovesLeft()) {
				byte move = (byte) moves.Dequeue();

				//change picture for this move back to empty
				GameObject g = (GameObject)moveBarQ.Dequeue ();
				g.GetComponent<Image> ().sprite = moveBorder;

				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				//Debug.Log("Move direction:" + move);
				if (move == UP) {
					AttemptMove<Wall> (0, 1);
				} else if (move == LEFT) {
					AttemptMove<Wall> (-1, 0);
				} else if (move == DOWN) {
					AttemptMove<Wall> (0, -1);
				} else if (move == RIGHT) {
					AttemptMove<Wall> (1, 0);
				} else if (move == ATTACK) {
					//split into attack up or down or right or left
					Attack (0,1);
				} else{
					Debug.Log("Something other that a direction was queued");
				}
					

			}
		}

		private void Update ()
		{		
			
			//This makes sure you cant add to the q until prepPhase.
			prepPhase = GameManager.instance.prepPhase;
			//enqueue the appropriate direction
			int count = moves.Count;
			if (count < maxMoves && prepPhase) {
				
				if (Input.GetKeyDown (KeyCode.UpArrow)) {
					moves.Enqueue (UP);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],UP);
					//Debug.Log ("UP Queued");
				} 
				else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					moves.Enqueue (LEFT);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],LEFT);
					//Debug.Log ("LEFT Queued");
				}
				else if (Input.GetKeyDown (KeyCode.DownArrow)) {
					moves.Enqueue (DOWN);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],DOWN);
					//Debug.Log ("DOWN Queued");
				} 
				else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					moves.Enqueue (RIGHT);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],RIGHT);
					//Debug.Log ("RIGHT Queued");
				}


				else if (Input.GetKeyDown (KeyCode.W)) {
					moves.Enqueue (UP);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],UP);
					//Debug.Log ("UP Queued");
				} 
				else if (Input.GetKeyDown (KeyCode.A)) {
					moves.Enqueue (LEFT);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],LEFT);
					//Debug.Log ("LEFT Queued");
				}
				else if (Input.GetKeyDown (KeyCode.S)) {
					moves.Enqueue (DOWN);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],DOWN);
					//Debug.Log ("DOWN Queued");
				} 
				else if (Input.GetKeyDown (KeyCode.D)) {
					moves.Enqueue (RIGHT);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],RIGHT);
					//Debug.Log ("RIGHT Queued");
				}
				else if (Input.GetKeyDown (KeyCode.Space)) {
					moves.Enqueue (ATTACK);
					moveBarQ.Enqueue (moveBar[count]);
					setMove(moveBar[count],ATTACK);
					//Debug.Log ("RIGHT Queued");
				}

			}
		}

		//try to attack
		protected bool Attack (int x, int y)
		{

			//Store start position to move from, based on objects current transform position.
			Vector2 start = transform.position;

			// Calculate end position based on the direction parameters passed in when calling Move.
			Vector2 end = start + new Vector2 (x, y);

			//Disable the boxCollider so that linecast doesn't hit this object's own collider.
			boxCollider.enabled = false;

			//Cast a line from start point to end point checking collision on blockingLayer.
			RaycastHit2D hit = Physics2D.Linecast (start, end, blockingLayer);

			//Re-enable boxCollider after linecast
			boxCollider.enabled = true;

			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");

			//Check if anything was hit
			if(hit.transform != null)
			{

				//check if its an enemy
				//If enemy hit kill it
				GameObject g= hit.transform.gameObject;
				if(g.tag.Equals("Enemy")){
					Enemy e1;
					Enemy2 e2;
					if (g.GetComponent<Enemy> () != null) {
						e1 = g.GetComponent<Enemy> ();
						killEnemy (g, e1);
						return true;
					} else {
						e2 = g.GetComponent<Enemy2> ();
						killEnemy (g, e2);
						return true;
					}
	
				}


			}

			//return false, attack was unsuccesful.
			return false;

		}

		private void killEnemy(GameObject enemy, MovingObject enemyscript){
			// dont show enemy
			enemy.SetActive(false);

			//remove the script from the list of enemies
			GameManager.instance.enemies.Remove (enemyscript);
			//enemy.moving = false;

		}

		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Every time player moves, subtract from food points total.
			//food--;
			
			//Update food text display to reflect current score.
			foodText.text = "Health: " + food;
			
			//Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
			base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//Since the player has moved and lost food points, check if the game has ended.
			CheckIfGameOver ();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			//GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
			//Check if the tag of the trigger collided with is Exit.
			if(other.tag == "Exit")
			{
				onExit = true;

				//Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
				Invoke ("Restart", restartLevelDelay);

				GameManager.instance.setup();
				//Disable the player object since level is over.
				enabled = false;
			}
			
			//Check if the tag of the trigger collided with is Food.
			else if(other.tag == "Food")
			{
				//Add pointsPerFood to the players current food total.
				addFood (pointsPerFood);
				
				//Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
				SoundManager.instance.RandomizeSfx (eatSound1, eatSound2);
				
				//Disable the food object the player collided with.
				other.gameObject.SetActive (false);
			}
			
			//Check if the tag of the trigger collided with is Soda.
			else if(other.tag == "Soda")
			{
				//Add pointsPerSoda to the players current food total.
				addFood (pointsPerSoda);

				
				//Call the RandomizeSfx function of SoundManager and pass in two drinking sounds to choose between to play the drinking sound effect.
				SoundManager.instance.RandomizeSfx (drinkSound1, drinkSound2);
				
				//Disable the soda object the player collided with.
				other.gameObject.SetActive (false);
			}
		}

		private void addFood(int points){
			//Add pointsPerSoda to players food points total
			food += points;

			//cant go over 100 food
			if (food > 100) {
				food = 100;
			}

			//health icon
			healthIcon.fillAmount = food/100F;

			//Update foodText to represent current total and notify player that they gained points
			foodText.text = "+" + points + " Health: " + food;
		}
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
			
			//Load the last scene loaded, in this case Main, the only scene in the game.
			SceneManager.LoadScene(0);
		}
		
		
		//LoseFood is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many points to lose.
		public void LoseFood (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			
			//Subtract lost food points from the players total.
			food -= loss;

			//health icon
			healthIcon.fillAmount = food/100F;
			
			//Update the food display with the new total.
			foodText.text = "-"+ loss + " Health: " + food;
			
			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of food points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if food point total is less than or equal to zero.
			if (food <= 0) 
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);
				
				//Stop the background music.
				SoundManager.instance.musicSource.Stop();
				
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();
			}
		}
	}
}

