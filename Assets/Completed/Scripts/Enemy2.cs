using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Enemy2 : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.
		public AudioClip attackSound2;						//Second of two audio clips to play when attacking the player.


		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.
		private bool skipMove;								//Boolean to determine whether or not enemy should skip a turn or move this turn.
		private bool skipShoot;								//Boolean to determine whether or not enemy should skip a turn or move this turn.

		public GameObject bullet;
		//public bool moving = true;


		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddEnemyToList (this);

			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();

			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;

			//Call the start function of our base class MovingObject.
			base.Start ();
		}


		//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Debug.Log(skipMove + " moving x: " + xDir +" y : " + yDir);
			//Check if skipMove is true, if so set it to false and skip this turn.
			if(skipMove)
			{
				skipMove = false;

				if (skipShoot) {
					skipShoot = false;
				} else {
					Attack ();
					skipShoot = true;
				}


				return;

			}
			//Debug.Log("moving x: " + xDir +" y : " + yDir);
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);

			//Now that Enemy has moved, set skipMove to true to skip next move.
			skipMove = true;
		}

		private void Attack(){
			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;


			float xDist = Mathf.Abs (target.position.x - transform.position.x);
			float yDist = Mathf.Abs (target.position.y - transform.position.y);

			if (xDist < yDist) {
				yDir = target.position.y > transform.position.y ? 1 : -1;
			} else {
				xDir = target.position.x > transform.position.x ? 1 : -1;
			}
			//Debug.Log("spawning bullet at x: " + transform.position.x + xDir +" y : " + transform.position.y + yDir);
			//Instantiate a bullet shooting towards the player if there isnt any colliderboxes where the bullet wants to go
			if (Physics2D.OverlapCircle (new Vector2 (transform.position.x + xDir, transform.position.y + yDir), 0.1F) == null) {
				GameObject instance =
					Instantiate (bullet, new Vector3 (transform.position.x + xDir, transform.position.y + yDir, 0f), Quaternion.identity) as GameObject;

				instance.GetComponent<Projectile> ().setDirs (xDir, yDir);
			} else {
				Debug.Log("ColliderBox already at x: " + transform.position.x + xDir +" y : " + transform.position.y + yDir);
			}
		}


		//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
		public void MoveEnemy ()
		{

			//Declare variables for X and Y axis move directions, these range from -1 to 1.
			//These values allow us to choose between the cardinal directions: up, down, left and right.
			int xDir = 0;
			int yDir = 0;

			bool xCloser;

			float xDist = Mathf.Abs (target.position.x - transform.position.x);
			float yDist = Mathf.Abs (target.position.y - transform.position.y);

			if (xDist < yDist) {
				xCloser = true;
			} else {
				xCloser = false;
			}
			if (xCloser) {
				//If the difference in positions is approximately zero (Epsilon) do the following:
				if (Mathf.Abs (target.position.x - transform.position.x) < 0.1) {

					//lined up in the x so run away in the y
					yDir = target.position.y > transform.position.y ? -1 : 1;

					//Debug.Log("move up or down " + yDir);

					//Line up in the x axis
				} else {
					//Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
					xDir = target.position.x > transform.position.x ? 1 : -1;

					//Debug.Log("left or right " + xDir);
				}
			} else {
				//If the difference in positions is approximately zero (Epsilon) do the following:
				if (Mathf.Abs (target.position.y - transform.position.y) < 0.1) {

					//lined up in the y so run away in the x
					xDir = target.position.x > transform.position.x ? -1 : 1;

					//Debug.Log("move up or down " + yDir);

					//Line up in the y axis
				} else {
					//Check if target y position is greater than enemy's x position, if so set y direction to 1 (move right), if not set to -1 (move left).
					yDir = target.position.y > transform.position.y ? 1 : -1;

					//Debug.Log("left or right " + xDir);
				}
			}
			//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
			AttemptMove <Player> (xDir, yDir);


		}


		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;

			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);

			//Set the attack trigger of animator to trigger Enemy attack animation.
			animator.SetTrigger ("enemyAttack");

			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			SoundManager.instance.RandomizeSfx (attackSound1, attackSound2);
		}
	}
}
