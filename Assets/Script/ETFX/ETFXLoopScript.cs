using UnityEngine;
using System.Collections;

namespace EpicToonFX
{
	public class ETFXLoopScript : MonoBehaviour
	{
		public GameObject chosenEffect;
		public float loopTimeLimit = 2.0f;
	
		[Header("Spawn without")]
	
		public bool spawnWithoutLight = true;
		public bool spawnWithoutSound = true;


		void Start ()
		{	
			PlayEffect();
		}

        private void Update()
        {

        }

        public void PlayEffect()
		{
			StartCoroutine("EffectLoop");
		}

		IEnumerator EffectLoop()
		{
			GameObject effectPlayer = (GameObject) Instantiate(chosenEffect, transform.position, transform.rotation);
			
			var lightComponent = effectPlayer.GetComponent<Light>();
			if (spawnWithoutLight && lightComponent)
			{
				lightComponent.enabled = false;
				//Destroy(gameObject.GetComponent<Light>());
				
			}
			
			var audioSource = effectPlayer.GetComponent<AudioSource>();
			if (spawnWithoutSound && audioSource)
			{
				audioSource.enabled = false;
				//Destroy(gameObject.GetComponent<AudioSource>());
			}
				
			yield return new WaitForSeconds(loopTimeLimit);

			Destroy (effectPlayer);
			PlayEffect();
		}
	}
}