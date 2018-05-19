using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Menu.Types; //PageType

namespace Menu {
	
	namespace Pages {

		/// <summary>
		/// The page manager is in charge of a set of page controllers
		/// It stores pages in a hashtable for quick access, and controls the enter and exit process for each page
		/// </summary>
		public class PageManager : MonoBehaviour {

			public static PageManager Instance;

			public PageController[] pages;
			public PageType entry;
			public float entryDelay;
			public bool isPersistent; //there may be page managers that do not persist between scenes
			Hashtable pageHash;
			List<PageType> offQueue;
			List<PageType> onQueue;

			private CustomYieldInstruction wait;

			void Awake() {
				if (isPersistent) {
					if (Instance != null) {
						Destroy(gameObject);
					}
					else {
						Instance = this;
						pageHash = new Hashtable();
						offQueue = new List<PageType>();
						onQueue = new List<PageType>();
						DontDestroyOnLoad(gameObject);
					}
				}
				else {
					pageHash = new Hashtable();
					offQueue = new List<PageType>();
					onQueue = new List<PageType>();
				}
			}

			/// <summary>
			/// Initialize the page hash based on the public pages collection
			/// </summary>
			void Start() {
                
				wait = null;
		
				StartCoroutine("RunPageManager");

				for (int i = 0; i < pages.Length; i++) {
                    if (pages[i] == null) continue;
					pageHash.Add(pages[i].pageType, pages[i]);
				}

				Init();
			}

			void OnDisable() {
				StopCoroutine("RunPageManager");
				Disable();
			}

			void OnEnable() {
				StartCoroutine("RunPageManager");
				Enable();
			}

			IEnumerator RunPageManager() {
				yield return new WaitForSeconds(entryDelay);
				if (entry != PageType.None) {
					TurnPageOn(PageType.None, entry, false);
				}
				
				while (true) {
					yield return wait;

					if (offQueue.Count > 0) {
						for (int i = 0; i < offQueue.Count; i++) {
							if (!PageIsTurningOff(offQueue[i])) {
								if (Page(offQueue[i]) == null) continue;
								if (!Page(offQueue[i]).gameObject.activeSelf)
								{
									ConfirmPageHasExited(offQueue[i]);
									continue;
								}
								Page(offQueue[i]).TurnOff();
							}
						}
					}
					else {
						for (int i = 0; i < onQueue.Count; i++) {
							if (!PageIsTurningOn(onQueue[i])) {
								if (Page(onQueue[i]) == null) continue;
								if (Page(onQueue[i]).gameObject.activeSelf)
								{
									ConfirmPageHasEntered(onQueue[i]);
									continue;
								}
								Page(onQueue[i]).gameObject.SetActive(true);
								Page(onQueue[i]).TurnOn();
							}
						}
					}

				}
			}

			/// <summary>
			/// Remove a page before turning on a new page
			/// </summary>
			public void TurnPageOn(PageType pageToRemove, PageType pageToLoad, bool synchronous) {
				if (onQueue.Contains(pageToLoad)) return;
				if (PageIsTurningOn(pageToLoad)) return;
				if (PageIsOn(pageToLoad))
				{
					//page is on, and not turning off
					if (!PageIsTurningOff(pageToLoad))
					{
						return;
					}
					//page is on, and is turning off
					else
					{
						//tell page to stop turning off
						offQueue.Remove(pageToLoad);
						Page(pageToLoad).StopTurningOff();
					}
				}

				if (pageToRemove == PageType.None) {
					if (synchronous) {
						Page(pageToLoad).gameObject.SetActive(true);
						Page(pageToLoad).TurnOn();
					}
					else {
						onQueue.Add(pageToLoad);
					}
				}
				else {
					StartCoroutine(WaitToLoadPage(pageToRemove, pageToLoad, synchronous));
				}
			}

			/// <summary>
			/// Disable a page. See PageController.cs
			/// </summary>
			public void TurnPageOff(PageType page, bool synchronous) {
				if (offQueue.Contains(page)) return;
				if (PageIsTurningOff(page)) return;
				if (!PageIsOn(page))
				{
					//page is off, and not turning on
					if (!PageIsTurningOn(page))
					{
						return;
					}
					//page is off, and is turning on
					else
					{
						//tell page to stop turning on
						onQueue.Remove(page);
						Page(page).StopTurningOn();
					}
				}

				if (synchronous) {
					Page(page).TurnOff();
				}
				else {
					offQueue.Add(page);
				}
			}

			public bool PageIsOn(PageType page) {
				if (Page(page) == null) return false;
				return Page(page).IsOn;
			}

			public bool PageIsTurningOn(PageType page) {
				if (Page(page) == null) return false;
				return Page(page).IsTurningOn;
			}

			public bool PageIsTurningOff(PageType page) {
				if (Page(page) == null) return false;
				return Page(page).IsTurningOff;
			}

			/// <summary>
			/// Intended use by PageController.cs
			/// </summary>
			public void ConfirmPageHasExited(PageType page) {
				if (!offQueue.Contains(page)) return;
				offQueue.Remove(page);
			}

			/// <summary>
			/// Intended use by PageController.cs
			/// </summary>
			public void ConfirmPageHasEntered(PageType page) {
				if (!onQueue.Contains(page)) return;
				onQueue.Remove(page);
			}

			public void CloseAllPages() {
				for (int i = 0; i < pages.Length; i++) {
                    //if (!PageIsOn(pages[i].pageType)) continue;
                    if (pages[i] == null) continue;
					TurnPageOff(pages[i].pageType, true);
				}
			}

			/// <summary>
			/// Wait until 'pageToRemove' is done with animating out before turning on 'pageToLoad'
			/// </summary>
			IEnumerator WaitToLoadPage(PageType pageToRemove, PageType pageToLoad, bool synchronous) {
				yield return wait;
				TurnPageOff(pageToRemove, synchronous);

				if (!synchronous)
				{
					while (Page(pageToRemove).IsOn) {
						yield return wait;
					}
				}

				yield return wait;
			
				if (!onQueue.Contains(pageToLoad) && !offQueue.Contains(pageToLoad)) {
					if (synchronous) {
						Page(pageToLoad).gameObject.SetActive(true);
						Page(pageToLoad).TurnOn();
					}
					else {
						onQueue.Add(pageToLoad);
					}
				}
			}

			/// <summary>
			/// Retrieve a page from the page hash if it exists
			/// </summary>
			PageController Page(PageType page) {
				if (!pageHash.Contains(page)) {
					Debug.LogWarning("The page you are trying to access could not be found => "+page);
					return null;
				} 
				return (PageController)pageHash[page];
			}

			public bool AnyPageIsOn() {
				for (int i = 0; i < pages.Length; i++) {
                    if (pages[i] == null) continue;
					if (PageIsOn(pages[i].pageType))
						return true;
				}
				return false;
			}

			public void ClearPagesWithFilter(PageType[] filter) {
				for (int i = 0; i < pages.Length; i++) {
					if (pages[i] == null) continue;
					if (!System.Array.Exists(filter, element => element == pages[i].pageType)) {
						TurnPageOff(pages[i].pageType, false);
					}
				}
			}

			public bool PagesMinusFilterAreOn(PageType[] filter) {
				for (int i = 0; i < pages.Length; i++) {
					if (pages[i] == null) continue;
					if (!System.Array.Exists(filter, element => element == pages[i].pageType) &&
						PageIsOn(pages[i].pageType)) {
						return true;
					}
				}
				return false;
			}

			public bool PageExists(PageType page) {
				if (pageHash == null) return false;
				if (!pageHash.Contains(page)) {
					return false;
				} 
				return true;
			}

			public virtual void Init() {}
			public virtual void Enable() {}
			public virtual void Disable() {}
		}
	}
}
