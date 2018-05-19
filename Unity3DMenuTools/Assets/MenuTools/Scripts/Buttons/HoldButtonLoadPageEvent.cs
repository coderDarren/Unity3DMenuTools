
using UnityEngine;

using Menu.Pages; //PageManager.cs
using Menu.Types; //PageType

namespace Menu {

	namespace Buttons {
		
		public class HoldButtonLoadPageEvent : HoldButtonEvent {

			public override void OnItemHold()
			{
				base.OnItemHold ();
				pageManager.TurnPageOn (pageToRemove, pageToLoad, synchronous);
			}

			public PageManager pageManager;
			public PageType pageToLoad;
			public PageType pageToRemove;
			public bool synchronous;
		}
	}
}
