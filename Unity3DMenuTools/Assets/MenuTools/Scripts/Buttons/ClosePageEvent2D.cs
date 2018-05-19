using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Menu.Pages; //PageManager.cs
using Menu.Types; //PageType

namespace Menu {
	namespace Buttons {

		public class ClosePageEvent2D : ButtonEvent {

			public PageManager pageManager;
			public PageType[] pagesToClose;
			public PageType[] pagesToOpen;
			public bool synchronous = false;

			public override void OnItemUp() {
				base.OnItemUp();
				RunAction();
			}

			void RunAction() {
				for (int i = 0; i < pagesToClose.Length; i++) {
					pageManager.TurnPageOff(pagesToClose[i], synchronous);
				}

				for (int i = 0; i < pagesToOpen.Length; i++) {
					pageManager.TurnPageOn(PageType.None, pagesToOpen[i], synchronous);
				}
			}
		}
	}
}
