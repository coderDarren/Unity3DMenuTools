using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Effects {
		
		[RequireComponent(typeof(CanvasGroup))]
		public class FadeUIController : MonoBehaviour {

			private IEnumerator Fade(float target)
			{
				float curr = m_canvas.alpha;
				float initial = curr;
				float time = 0;

				while (curr != target)
				{
					curr = Mathf.Lerp(initial, target, time / m_smooth);
					m_canvas.alpha = curr;
					time += Time.deltaTime;
					yield return null;
				}
			}

			public void FadeIn(float smoothTime)
			{
				if (m_on) return;
				m_smooth = smoothTime;
				m_on = true;
				StopCoroutine("Fade");
				StartCoroutine("Fade", 1);
			}

			public void FadeOut(float smoothTime)
			{
				if (!m_on) return;
				m_smooth = smoothTime;
				m_on = false;
				StopCoroutine("Fade");
				StartCoroutine("Fade", 0);
			}

			private void OnEnable()
			{
				m_canvas = GetComponent<CanvasGroup>();
			}

			private CanvasGroup m_canvas;
			private bool m_on;
			private float m_smooth;
		}
	}
}
