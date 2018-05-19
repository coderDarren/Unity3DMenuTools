using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Effects {

		[RequireComponent(typeof(Image))]
		public class ImageFillCycler : MonoBehaviour {

			private IEnumerator CycleFill()
			{
				float _timer = 0;
				float _targetFill = 1;

				while (true)
				{
					while (m_fill != _targetFill)
					{
						m_fill = Mathf.Lerp(1 - _targetFill, _targetFill, _timer / fillSmooth);
						m_img.fillAmount = m_fill;
						_timer += Time.deltaTime;
						yield return null;
					}

					yield return new WaitForSeconds(cycleDelay);
					m_img.fillClockwise = !m_img.fillClockwise;
					_timer = 0;
					_targetFill = -_targetFill + 1;
				}
			}

			private void OnEnable()
			{
				m_img = GetComponent<Image>();
				m_fill = m_img.fillAmount;
				StartCoroutine("CycleFill");
			}

			private void OnDisable()
			{
				StopCoroutine("CycleFill");
			}

			public float fillSmooth;
			public float cycleDelay;

			private Image m_img;
			private float m_fill;

		}
	}
}
