using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Menu.Types; //PageType

namespace Menu {
	
	namespace Pages {

		/// <summary>
		/// Managed by PageManager.cs
		/// Responsible for executing entry and exit animations for this page
		/// </summary>
		[RequireComponent(typeof(Animator))]
		[RequireComponent(typeof(CanvasGroup))]
		public class PageController : MonoBehaviour {

			public PageType pageType;
			public PageType nextPage;
			public PageManager manager;
			
			bool isOn = false;
			bool isTurningOn = false;
			bool isTurningOff = false;
			CanvasGroup canvas;
			Animator anim;

			private CustomYieldInstruction wait;

			public bool IsOn { get { return isOn; } }
			public bool IsTurningOn { get { return isTurningOn; } }
			public bool IsTurningOff { get { return isTurningOff; } }

			void Awake() {
				AwakeInit();
			}
			
			/// <summary>
			/// Initialize
			/// </summary>
			void Start() {
				wait = null;
				Init();
			}

			/// <summary>
			/// Run the entry animation sequence
			/// </summary>
			public void TurnOn() {
				if (isOn) return;
				OnPageBeginEnter();
				StopCoroutines();
				StartCoroutine("RunEntrySequence");
			}

			public void StopTurningOn() {
				if (!isTurningOn) return;
				StopCoroutines();
				isTurningOn = false;
				isOn = false;
			}

			/// <summary>
			/// Make the page interactable and tell the page to animate into the game
			/// Do not set isOn to true until the page animation is done
			/// </summary>
			IEnumerator RunEntrySequence() {
				SetAnimState(true, false);

				isTurningOn = true;

				while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Entry")) yield return wait;

				while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || anim.IsInTransition(0)) { //is the animation over?
					yield return wait;
				}

				if (nextPage != PageType.None)
					manager.TurnPageOn(PageType.None, nextPage, false);

				OnPageEnter();
				isOn = true;
				manager.ConfirmPageHasEntered(pageType);
				isTurningOn = false;
				SetInteractability(true);
			}

			public void TurnOff() {
				OnPageBeginExit();
				StopCoroutines();
				StartCoroutine("RunExitSequence");
			}

			public void StopTurningOff()
			{
				if (!isTurningOff) return;
				StopCoroutines();
				isTurningOff = false;
				isOn = true;
			}

			/// <summary>
			/// Disable interactability with this page and tell the page to animate out of the game
			/// Do not set isOn to false until the page animation is done
			/// </summary>
			IEnumerator RunExitSequence() {
				SetAnimState(false, true);
				SetInteractability(false);

				isTurningOff = true;

				while (!anim.GetCurrentAnimatorStateInfo(0).IsName("Exit")) yield return wait;

				while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 || anim.IsInTransition(0)) { //is the animation over?
					yield return wait;
				}

				OnPageExit();
				canvas.alpha = 0;
				isOn = false;
				manager.ConfirmPageHasExited(pageType);
				isTurningOff = false;
				gameObject.SetActive(false);
			}

			/// <summary>
			/// Set animator parameters to determine the animation state
			/// </summary>
			void SetAnimState(bool enterStatus, bool exitStatus) {
				anim.SetBool("Enter", enterStatus);
				anim.SetBool("Exit", exitStatus);
			}

			/// <summary>
			/// Set the page interaction state. This will determine if the page listens for any tap events
			/// </summary>
			void SetInteractability(bool canInteract) {
				canvas.interactable = canInteract;
				canvas.blocksRaycasts = canInteract;
				Collider[] colliders = GetComponentsInChildren<Collider>();
				foreach (Collider col in colliders) {
					col.enabled = canInteract;
				}
			}

			private void StopCoroutines() {
				StopCoroutine("RunEntrySequence");
				StopCoroutine("RunExitSequence");
			}

			public virtual void Init() {
				
			}

			public virtual void AwakeInit() {
				anim = GetComponent<Animator>();
				canvas = GetComponent<CanvasGroup>();
				SetInteractability(false);
			}

			public virtual void OnPageBeginEnter() {
				Debug.Log("["+pageType+"] is entering.");
			}

			public virtual void OnPageBeginExit() {
				Debug.Log("["+pageType+"] is exiting.");
			}

			public virtual void OnPageEnter() {}
			public virtual void OnPageExit() {}
		}
	}
}
