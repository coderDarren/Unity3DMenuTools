
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Buttons {
		
		public class HoldButtonEvent : ButtonEvent {

			public override void OnItemUp()
			{
				base.OnItemUp ();
				StopCoroutine ("Hold");
				SetFill (0);
			}

			public override void OnItemDown()
			{
				base.OnItemDown ();
				StopCoroutine ("Hold");
				StartCoroutine ("Hold");
			}

			//called when the hold operation has completed
			public virtual void OnItemHold() 
			{
				m_holdTime = 0;
				m_nTime = m_holdTime / holdRequirement;
				SetFill (0);
			}

			private void SetFill(float amount)
			{
				if (fill == null)
					return;
				
				amount = Mathf.Clamp (amount, 0, 1);
				fill.fillAmount = amount;
			}

			private IEnumerator Hold()
			{
				m_holdTime = 0;
				m_nTime = m_holdTime / holdRequirement;
				SetFill (0);

				yield return new WaitForSeconds (delay);

				while (m_nTime < 1) {
					m_holdTime += Time.deltaTime;
					m_nTime = m_holdTime / holdRequirement;

					SetFill (m_nTime);

					yield return null;
				}

				OnItemHold (); //intent for inheriting members
			}

			private float m_holdTime;
			private float m_nTime;

			public float holdRequirement;
			public float delay;
			public Image fill;
		}
	}
}
