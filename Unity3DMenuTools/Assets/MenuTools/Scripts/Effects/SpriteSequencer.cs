using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Effects {

		[RequireComponent(typeof(Image))]
		public class SpriteSequencer : MonoBehaviour {

			private IEnumerator RunSequence()
			{
				int m_index = 0;

				if (m_sprites != null && m_sprites.Length > 0)
				{
					m_img.sprite = (Sprite)m_sprites[m_index];

					while (m_index < m_sprites.Length)
					{

						yield return m_wait;

						m_index++;
						m_img.sprite = (Sprite)m_sprites[m_index];

						if (loop && m_index > m_sprites.Length - 2)
						{
							m_index = 0;
						}
					}
				}

				else
				{
					yield return null;
				}
			}

			private void OnEnable()
			{
				m_img = GetComponent<Image>();
				m_sprites = Resources.LoadAll(resourcePath, typeof(Sprite));
				m_wait = new WaitForSeconds(sequenceRate);
				StartCoroutine("RunSequence");
			}

			private void OnDisable()
			{
				StopCoroutine("RunSequence");
			}

			private Object[] m_sprites;
			private Image m_img;
			private YieldInstruction m_wait;

			public string resourcePath;
			public float sequenceRate;
			public bool loop;
		}	
	}
}
