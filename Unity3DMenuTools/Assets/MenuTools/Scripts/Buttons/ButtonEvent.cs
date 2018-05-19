using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Menu {

	namespace Buttons {

		[RequireComponent(typeof(Image))]
		[RequireComponent(typeof(RectTransform))]
		public class ButtonEvent : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

			public enum ActionTiming {
				OnItemUp,
				OnItemDown,
				OnItemEnter,
				OnItemExit
			}

			//--------------- EVENTS ---------------
			//Note these events are not static.
			//A class may subscribe to a button's reference event.
			public delegate void ButtonEventDelegate();
			public event ButtonEventDelegate OnButtonUp;
			public event ButtonEventDelegate OnButtonDown;

			//--------------- OPTIONS ---------------

			[System.Serializable]
			public class SpriteOptions
			{
				public bool useSpriteSwap;

				public Sprite restSprite;
				public Sprite clickSprite;
				public Sprite hoverSprite;
			}

			[System.Serializable]
			public class ColorOptions
			{
				public bool useColorSwap;

				public Color restColor;
				public Color clickColor;
				public Color hoverColor;
				public Color inactiveColor;

				public float transitionTime;
				public bool setColorInstant;
			}

			[System.Serializable]
			public class OrientationOptions
			{
				public bool useOrientationSwap;

				public Orientation restOrientation;
				public Orientation clickOrientation;
				public Orientation hoverOrientation;

				public float transitionTime;
			}

			[System.Serializable]
			public struct Orientation
			{
				public Vector3 euler;
				public Vector3 scale;
			}

			[System.Serializable]
			public class EffectOptions
			{
				public EffectOne effectOne;
				public EffectTwo effectTwo;
				public EffectThree effectThree;
			}

			[System.Serializable]
			public class EffectOne : Effect
			{
				public bool persist;
			}

			[System.Serializable]
			public class EffectTwo : Effect
			{
				public float repeatSpeed;
			}

			[System.Serializable]
			public class EffectThree : Effect
			{
				public Image fillImage2;
				public float alternateSpeed;
			}

			[System.Serializable]
			public class Effect 
			{
				public bool useEffect;
				public float fillSpeed;
				public float dissolveSpeed;
				public Image fillImage;
			}

			//--------------- PUBLIC MEMBERS ---------------
			public string buttonName;
			public SpriteOptions 	  spriteOptions;
			public ColorOptions 	  colorOptions;
			public OrientationOptions orientationOptions;
			public EffectOptions 	  effectOptions;
			public Image			  overrideImage;

			//[HideInInspector]
			public bool interactable = true;

			//--------------- PROTECTED MEMBERS ---------------
			protected Image _image;

			//--------------- PRIVATE MEMBERS ---------------
			RectTransform   _rect;

			protected bool _onItem; //set during enter and exit events
			protected bool _down;
			protected bool _drag;
			protected bool _effectIsRunning;

			enum ButtonEffect
			{
				NONE,
				ONE,
				TWO,
				THREE
			}
			ButtonEffect _activeEffect;

			void Awake() {
				_image = GetComponent<Image>();
				if (overrideImage != null) {
					_image = overrideImage;
				}
				_rect = GetComponent<RectTransform>();
				ApplyItemModifications(spriteOptions.restSprite, colorOptions.restColor, orientationOptions.restOrientation);

				if (effectOptions.effectOne.useEffect)
					_activeEffect = ButtonEffect.ONE;
				else if (effectOptions.effectTwo.useEffect)
					_activeEffect = ButtonEffect.TWO;
				else if (effectOptions.effectThree.useEffect)
					_activeEffect = ButtonEffect.THREE;

			}

			void Start()
			{
				InitButton();
			}

			void OnDisable() {
				if (colorOptions.useColorSwap)
					_image.color = colorOptions.restColor;
			}

			public virtual void InitButton()
			{
				
			}

			#region --------------- UTILITY MEMBERS ---------------

			///Set the sprite of the image component if sprite switching is enabled
			void SetSprite(Sprite _sprite)
			{
				if (spriteOptions.useSpriteSwap)
				{
					_image.sprite = _sprite;
				}
			}

			///Set the color of the image component if color switching is enabled
			void SetColor(Color _color)
			{
				if (colorOptions.useColorSwap)
				{
					StopCoroutine("MoveColor");
					StartCoroutine("MoveColor", _color);
				}
			}

			//Set the rotation and scale of the component if orienting is enabled
			void SetOrientation(Orientation _orientation)
			{
				if (orientationOptions.useOrientationSwap)
				{
					//Debug.Log("Moving orientation");
					StopCoroutine("MoveRotation");
					StartCoroutine("MoveRotation", _orientation);
					StopCoroutine("MoveScale");
					StartCoroutine("MoveScale", _orientation);
				}
			}

			//Sets sprite, color and orientation when an event occurs
			void ApplyItemModifications(Sprite _sprite, Color _color, Orientation _orientation)
			{
				SetSprite(_sprite);
				SetColor(_color);
				SetOrientation(_orientation);
			}

			//Transitions the image's current color to _color in _time amount of time
			IEnumerator MoveColor(Color _color)
			{
				Color initial = _image.color;
				float timePassed = 0;

				while (_image.color != _color)
				{
					timePassed += Time.deltaTime;

					_image.color = Color.Lerp(initial, _color, timePassed / colorOptions.transitionTime);
					yield return null;
				}
				_image.color = _color;
			}

			//Transitions the rect's euler angles
			IEnumerator MoveRotation(Orientation _orientation)
			{
				float timePassed = 0;
				Vector3 _eulers = _rect.eulerAngles;
				Vector3 initial = _eulers;

				while (_eulers != _orientation.euler)
				{
					timePassed += Time.deltaTime;

					_eulers.x = Mathf.LerpAngle(initial.x, _orientation.euler.x, timePassed / orientationOptions.transitionTime);
					_eulers.y = Mathf.LerpAngle(initial.y, _orientation.euler.y, timePassed / orientationOptions.transitionTime);
					_eulers.z = Mathf.LerpAngle(initial.z, _orientation.euler.z, timePassed / orientationOptions.transitionTime);
					_rect.eulerAngles = _eulers;

					yield return null;
				}
				_rect.eulerAngles = _orientation.euler;
			}

			//Transitions the rect's scale
			IEnumerator MoveScale(Orientation _orientation)
			{
				float timePassed = 0;
				Vector3 _scale = _orientation.scale;
				Vector3 initial = _rect.localScale;

				while (_rect.localScale != _scale)
				{
					timePassed += Time.deltaTime;

					_rect.localScale = Vector3.Lerp(initial, _scale, timePassed / orientationOptions.transitionTime);

					yield return null;
				}
				_rect.localScale = _scale;
			}

			IEnumerator RunEffectOne()
			{
				EffectOne effect = effectOptions.effectOne;
				bool run = true;
				bool fill = true;
				Color color = effect.fillImage.color;

				color.a = 1;
				effect.fillImage.color = color;
				effect.fillImage.fillAmount = 0;

				_effectIsRunning = true;

				while (run)
				{
					if (fill) //image is filling up
					{
						effect.fillImage.fillAmount += effect.fillSpeed * Time.deltaTime;

						if (effect.fillImage.fillAmount >= 0.99f)
						{
							fill = effect.persist;
							if (!_onItem && !_drag)
								fill = false; //end the effect if cursor is not on the item at this point
						}
					}
					else //image is fading out
					{
						color.a -= effect.dissolveSpeed * Time.deltaTime;
						effect.fillImage.color = color;

						if (color.a <= 0.01f)
						{
							color.a = 0;
							effect.fillImage.color = color;
							run = false;
						}
					}

					yield return new WaitForSeconds(Time.deltaTime);
				}

				_effectIsRunning = false;
			}

			IEnumerator RunEffectTwo()
			{
				EffectTwo effect = effectOptions.effectTwo;
				bool run = true;
				bool fill = true;
				Color color = effect.fillImage.color;

				color.a = 1;
				effect.fillImage.color = color;
				effect.fillImage.fillAmount = 0;

				_effectIsRunning = true;

				while (run)
				{
					if (fill) //image is filling up
					{
						effect.fillImage.fillAmount += effect.fillSpeed * Time.deltaTime;

						if (effect.fillImage.fillAmount >= 0.99f)
						{
							fill = false;
						}
					}
					else //image is fading out
					{
						color.a -= effect.dissolveSpeed * Time.deltaTime;
						effect.fillImage.color = color;

						if (color.a <= 0.01f)
						{
							color.a = 0;
							effect.fillImage.color = color;

							yield return new WaitForSeconds(effect.repeatSpeed);

							effect.fillImage.fillAmount = 0;
							color.a = 1;
							effect.fillImage.color = color;
							fill = true;

							if (!_onItem && !_drag)
								run = false; //end the effect if cursor is not on the item at this point
						}
					}

					yield return new WaitForSeconds(Time.deltaTime);
				}

				_effectIsRunning = false;
			}

			/// <summary>
			/// This is an unfinished function
			/// </summary>
			IEnumerator RunEffectThree()
			{
				//EffectThree effect = effectOptions.effectThree;

				_effectIsRunning = true;

				while (_effectIsRunning)
				{
					yield return null;
				}

				_effectIsRunning = false;
			}

			IEnumerator EndEffect()
			{
				//wait for effects to stop if they are running
				//effect will force quit when hover is not true on the button
				//this prevents "buggy" looking behavior if we ask the button to stop an effect midway into the effect
				while (_effectIsRunning)
				{
					yield return null; 
				}

				Effect effect = _activeEffect == ButtonEffect.ONE ? (Effect)effectOptions.effectOne :
								_activeEffect == ButtonEffect.TWO ? (Effect)effectOptions.effectTwo :
								_activeEffect == ButtonEffect.THREE ? (Effect)effectOptions.effectThree : null;

				bool run = true;
				Color color = effect.fillImage.color;

				while (run && !_onItem)
				{
					color.a -= effect.dissolveSpeed * Time.deltaTime;
					effect.fillImage.color = color;

					if (color.a <= 0.01f)
					{
						color.a = 0;
						effect.fillImage.color = color;
						run = false;
					}

					yield return new WaitForSeconds(Time.deltaTime);
				}
			}

			protected void HandleButtonEffect()
			{
				if (_drag) //dont trigger an effect when the user is interacting with the button
					return;

				if (effectOptions.effectOne.useEffect)
					_activeEffect = ButtonEffect.ONE;
				else if (effectOptions.effectTwo.useEffect)
					_activeEffect = ButtonEffect.TWO;
				else if (effectOptions.effectThree.useEffect)
					_activeEffect = ButtonEffect.THREE;

				switch (_activeEffect)
				{
					case ButtonEffect.ONE:
						StopCoroutine("RunEffectOne");
						StartCoroutine("RunEffectOne");
						break;
					case ButtonEffect.TWO:
						StopCoroutine("RunEffectTwo");
						StartCoroutine("RunEffectTwo");
						break;
					case ButtonEffect.THREE:
						StopCoroutine("RunEffectThree");
						StartCoroutine("RunEffectThree");
						break;
				}
			}

			protected void StopButtonEffect()
			{
				if (_drag) //dont trigger an effect when the user is interacting with the button
					return;

				if (_activeEffect != ButtonEffect.NONE)
				{
					StopCoroutine("EndEffect");
					StartCoroutine("EndEffect");
				}
			}

			protected void ApplyRestProperties()
			{
				ApplyItemModifications(spriteOptions.restSprite, colorOptions.restColor, orientationOptions.restOrientation);
			}

			protected void ApplyHoverProperties()
			{
				ApplyItemModifications(spriteOptions.hoverSprite, colorOptions.hoverColor, orientationOptions.hoverOrientation);
			}

			protected void ApplyClickProperties()
			{
				ApplyItemModifications(spriteOptions.clickSprite, colorOptions.clickColor, orientationOptions.clickOrientation);
			}

			protected void SetActive(bool active) {
				interactable = active;
				if(!interactable) {
					if (colorOptions.useColorSwap) _image.color = colorOptions.inactiveColor;
				} else {
					if (colorOptions.useColorSwap) _image.color = colorOptions.restColor;
				}
			}

			#endregion

			#region --------------- VIRTUAL MEMBERS ---------------

			/// Members below are intended to be overriden by more specific button functionality
			/// All buttons, however, will apply item modifications based on the parameters set from the inspector

			public virtual void OnItemDrag(PointerEventData ped)
			{
				_drag = true;
				ApplyItemModifications(spriteOptions.clickSprite, colorOptions.clickColor, orientationOptions.clickOrientation);
				Debug.Log("drag");
				//CancelInvoke();
			}

            public virtual void OnItemEndDrag(PointerEventData ped)
            {}

			public virtual void OnItemDown()
			{
				Debug.Log("down");
				_down = true;
				ApplyItemModifications(spriteOptions.clickSprite, colorOptions.clickColor, orientationOptions.clickOrientation);
				
				if(colorOptions.setColorInstant) {
					_image.color = colorOptions.clickColor;
				}
			}

			/// Represents a click event, when the user releases input on a button
			/// If the user is not on the item, no click logic will execute
			public virtual void OnItemUp()
			{
				Debug.Log("up");
				_drag = false;
				_down = false;
				if (!_onItem)
				{
					ApplyItemModifications(spriteOptions.restSprite, colorOptions.restColor, orientationOptions.restOrientation);
					return;
				}
				else {
					ApplyItemModifications(spriteOptions.hoverSprite, colorOptions.hoverColor, orientationOptions.hoverOrientation);
				}
				
			}

			public virtual void OnItemEnter()
			{
				_onItem = true;
				ApplyHoverProperties();
				Debug.Log("enter");

				//#if UNITY_EDITOR && !(UNITY_ANDROID || UNITY_IOS)
				HandleButtonEffect();
				//#endif
			}

			public virtual void OnItemExit()
			{
				_onItem = false;
				ApplyRestProperties();
				
				//#if UNITY_EDITOR && !(UNITY_ANDROID || UNITY_IOS)
				StopButtonEffect();
				//#endif
				
				Debug.Log("exit");
			}

			#endregion


			#region --------------- INTERFACE MEMBERS ---------------

			///Members below call the virtual functions above
			///The base functionality for occurring events is derived from the functions above
			///Any added functionality would be placed in an inheriting class which overrides the virtual functions above

			public void OnDrag(PointerEventData ped)
			{
				if (!interactable) //interaction is not enabled, so we do not continue
					return;

				OnItemDrag(ped);
			}

			public void OnEndDrag(PointerEventData ped)
			{
				OnItemEndDrag(ped);
			}

			public void OnPointerDown(PointerEventData ped)
			{
				if (!interactable) //interaction is not enabled, so we do not continue
					return;

				try {
					//event for foreign classes to conduct some action to this button's object reference
					OnButtonDown();
				} catch (System.NullReferenceException) {}
				OnItemDown();
			}

			public void OnPointerUp(PointerEventData ped)
			{
				if (!interactable) //interaction is not enabled, so we do not continue
					return;

				try {
					//event for foreign classes to conduct some action to this button's object reference
					OnButtonUp();
				} catch (System.NullReferenceException) {}
				OnItemUp();
			}

			public void OnPointerEnter(PointerEventData ped)
			{
				if (!interactable) //interaction is not enabled, so we do not continue
					return;

				OnItemEnter();
			}

			public void OnPointerExit(PointerEventData ped)
			{
				if (!interactable) //interaction is not enabled, so we do not continue
					return;
					
				OnItemExit();
			}

			#endregion

		}
	}
}
