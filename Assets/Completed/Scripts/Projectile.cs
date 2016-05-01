using UnityEngine;
using System.Collections;

namespace Completed
{
	//Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
	public class Projectile : MovingObject
	{
		public int playerDamage; 							//The amount of food points to subtract from the player when attacking.
		public AudioClip attackSound1;						//First of two audio clips to play when attacking the player.

		private Animator animator;							//Variable of type Animator to store a reference to the enemy's Animator component.
		private Transform target;							//Transform to attempt to move toward each turn.

		int xDir = 0;
		int yDir = 1;

		//Start overrides the virtual Start function of the base class.
		protected override void Start ()
		{
			//Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
			//This allows the GameManager to issue movement commands.
			GameManager.instance.AddBulletToList (this);

			//Get and store a reference to the attached Animator component.
			animator = GetComponent<Animator> ();

			//Find the Player GameObject using it's tag and store a reference to its transform component.
			target = GameObject.FindGameObjectWithTag ("Player").transform;

			//Call the start function of our base class MovingObject.
			base.Start ();
		}

		public void setDirs(int x, int y){
			xDir = x;
			yDir = y;
		}


		//Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
		//See comments in MovingObject for more on how base AttemptMove function works.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Debug.Log("moving x: " + xDir +" y : " + yDir);
			//Call the AttemptMove function from MovingObject.
			base.AttemptMove <T> (xDir, yDir);

			if (!canMove) {
				this.gameObject.SetActive (false);
				GameManager.instance.removeBulletfromList (this);
			}
		}


		//MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
		public void MoveBullet ()
		{



			//Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
			AttemptMove <Player> (xDir, yDir);


		}
			

		//OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
		//and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
		protected override void OnCantMove <T> (T component)
		{
			Debug.Log("hit player");
			//Declare hitPlayer and set it to equal the encountered component.
			Player hitPlayer = component as Player;

			//Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
			hitPlayer.LoseFood (playerDamage);

			//Set the attack trigger of animator to trigger Enemy attack animation.
			//animator.SetTrigger ("enemyAttack");


			//Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
			//SoundManager.instance.RandomizeSfx (attackSound1);
		}
	}
}
