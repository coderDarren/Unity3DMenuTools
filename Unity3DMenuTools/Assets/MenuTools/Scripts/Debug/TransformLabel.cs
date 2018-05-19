
using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Debugging {
	
		[RequireComponent(typeof(Text))]
		public class TransformLabel : MonoBehaviour {

			private void Start()
			{
				m_label = GetComponent<Text> ();
			}

			private void Update()
			{
				if (obj == null)
					return;

				m_label.text = "<b>"+prefix+"</b>\nPosition: (" + obj.position.x.ToString("0.##") + "," + obj.position.y.ToString("0.##") + "," + obj.position.z.ToString("0.##") + ")\n"+
							   "Rotation: (" + obj.eulerAngles.x.ToString("0.##") + "," + obj.eulerAngles.y.ToString("0.##") + "," + obj.eulerAngles.z.ToString("0.##") + ")";

			}

			private Text m_label;

			public Transform obj;
			public string prefix;
		}
	}
}
