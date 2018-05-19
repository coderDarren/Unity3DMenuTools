using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace Menu {

	namespace Layouts {

		[ExecuteInEditMode]
		public class VerticalLayoutGroup : MonoBehaviour {

			/// <summary>
			/// A function for derivative classes.
			/// </summary>
			public virtual void OnLayoutUpdate() 
			{
				Debug.Log("Vertical Layout Did Update");
			}

			/// <summary>
			/// Update the children rects in accordance with the layout settings
			/// </summary>
			private void UpdateLayout()
			{
				ushort _index = 0;
				float _containerWidth = m_container.rect.width;
				SetRectHeight(m_container, 0);
				foreach (Transform _t in transform)
				{
					AddRectHeight(m_container, height + padding);
					RectTransform _rect = _t.GetComponent<RectTransform>();
					SetRectSize(_rect, _containerWidth - padding * 4, height);
					SetRectPosition(_rect, (_containerWidth / 2.0f) - padding, (-height * _index) -  padding - (padding *_index) - (height / 2.0f));
					_index++;
				}

				OnLayoutUpdate();
			}

			private void SetRectSize(RectTransform _rect, float _width, float _height)
			{
				Vector2 _size = _rect.sizeDelta;
				_size.x = _width;
				_size.y = _height;
				_rect.sizeDelta = _size;
			}

			private void AddRectHeight(RectTransform _rect, float _height)
			{
				Vector2 _size = _rect.sizeDelta;
				_size.y += _height;
				_rect.sizeDelta = _size;
			}

			private void SetRectHeight(RectTransform _rect, float _height)
			{
				Vector2 _size = _rect.sizeDelta;
				_size.y = _height;
				_rect.sizeDelta = _size;
			}

			private void SetRectPosition(RectTransform _rect, float _x, float _y)
			{
				Vector3 _pos = _rect.localPosition;
				_pos.x = _x;
				_pos.y = _y;
				_rect.localPosition = _pos;
			}

			/// <summary>
			/// Track children changed event
			/// </summary>
			private void Update()
			{
				m_previousChildren = m_children;
				m_children = transform.childCount;

				if (m_previousChildren != m_children)
				{
					UpdateLayout();
				}
			}

			private void Start()
			{
				m_container = GetComponent<RectTransform>();
			}

			[Header("Layout")]
			public float height;
			public float padding;

			private RectTransform m_container;
			private int m_children;
			private int m_previousChildren;

		}
	}
}
