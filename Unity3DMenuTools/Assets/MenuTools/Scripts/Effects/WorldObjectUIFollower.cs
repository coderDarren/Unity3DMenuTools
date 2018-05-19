
using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Effects {

		/// <summary>
		/// This class to be used on a UI element that follows 3D space objects on a 2D menu.
		/// </summary>
		[RequireComponent(typeof(RectTransform))]
		public class WorldObjectUIFollower : MonoBehaviour {

			private void Update()
			{
				transform.position = Camera.main.WorldToScreenPoint(worldObject.position);
			}

			public Transform worldObject;
		}
	}
}
