using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mga
{
	[RequireComponent(typeof(Image))]
	public class CPlayedCardOnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public bool dragOnSurfaces = true;
		public ScrollRect m_ScrollRect = null;
		public CFixGridRect m_FixGridRect = null;
		private GameObject m_DraggingCard;
		private RectTransform m_DraggingPlane;

		public bool isVertical = false;
		private bool isSelf = false;

		private System.Action m_OnBeginDragCallBack = null;
		private System.Action m_OnEndDragCallBack = null;

		private System.Action m_OnBeginScroll = null;
		private System.Action m_OnEndScroll = null;
		public void Init(CLocalCharInfo roleInfo, System.Action beginCallBack = null, System.Action endCallBack = null, System.Action beginScroll = null, System.Action endScroll = null)
		{
			m_OnBeginDragCallBack = beginCallBack;
			m_OnEndDragCallBack = endCallBack;
			m_OnBeginScroll = beginScroll;
			m_OnEndScroll = endScroll;
		}
		public void OnBeginDrag(PointerEventData eventData)
		{
			Vector2 touchDeltaPosition = Vector2.zero;
#if UNITY_EDITOR
			float delta_x = Input.GetAxis("Mouse X");
			float delta_y = Input.GetAxis("Mouse Y");
			touchDeltaPosition = new Vector2(delta_x, delta_y);

#elif UNITY_ANDROID || UNITY_IPHONE
        touchDeltaPosition = Input.GetTouch(0).deltaPosition;  
#endif
			if (isVertical)
			{
				if (Mathf.Abs(touchDeltaPosition.x) > Mathf.Abs(touchDeltaPosition.y))
				{
					isSelf = true;
					var canvas = FindInParents<Canvas>(gameObject);
					if (canvas == null)
						return;
					m_DraggingCard = createCard();
					m_DraggingCard.transform.SetAsLastSibling();

					m_DraggingCard.AddComponent<CIgnoreRayCast>();

					if (dragOnSurfaces)
						m_DraggingPlane = transform as RectTransform;
					else
						m_DraggingPlane = canvas.transform as RectTransform;

					SetDraggedPosition(eventData);
					if (m_OnBeginDragCallBack != null)
					{
						m_OnBeginDragCallBack();
					}
				}
				else
				{
					isSelf = false;
					if (m_ScrollRect != null)
						m_ScrollRect.OnBeginDrag(eventData);
				}
			}
			else
			{
				if (Mathf.Abs(touchDeltaPosition.x) < Mathf.Abs(touchDeltaPosition.y))
				{
					isSelf = true;
					var canvas = FindInParents<Canvas>(gameObject);
					if (canvas == null)
						return;
					m_DraggingCard = createCard();
					m_DraggingCard.transform.SetAsLastSibling();

					m_DraggingCard.AddComponent<CIgnoreRayCast>();

					if (dragOnSurfaces)
						m_DraggingPlane = transform as RectTransform;
					else
						m_DraggingPlane = canvas.transform as RectTransform;

					SetDraggedPosition(eventData);
					if (m_OnBeginDragCallBack != null)
					{
						m_OnBeginDragCallBack();
					}
				}
				else
				{
					isSelf = false;
					if (m_ScrollRect != null)
						m_ScrollRect.OnBeginDrag(eventData);
				}
			}
			if (m_OnBeginScroll != null)
				m_OnBeginScroll();
		}

		public void OnDrag(PointerEventData data)
		{
			if (isSelf)
			{
				if (m_DraggingCard != null)
				{
					SetDraggedPosition(data);
				}
			}
			else
			{
				if (m_ScrollRect != null)
					m_ScrollRect.OnDrag(data);
			}
		}

		private void SetDraggedPosition(PointerEventData data)
		{
			if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
				m_DraggingPlane = data.pointerEnter.transform as RectTransform;

			var rt = m_DraggingCard.GetComponent<RectTransform>();
			Vector3 globalMousePos;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
			{
				rt.position = globalMousePos;
				rt.rotation = m_DraggingPlane.rotation;
			}
		}

		private GameObject createCard()
		{
			CUIPlayedCharCardWidget charCardWidget = null;
			m_DraggingCard = Instantiate(Resources.Load<GameObject>("Prefab/RoleCard") as GameObject);
			if (m_DraggingCard == null)
			{
				return null;
			}
			charCardWidget = m_DraggingCard.GetComponent<CUIPlayedCharCardWidget>();

			if (charCardWidget == null)
			{
				return null;
			}

			m_DraggingCard.transform.parent = FindInParents<Canvas>(gameObject).transform;
			m_DraggingCard.transform.localScale = Vector3.one;
			CUIPlayedCharCardWidget.CUIContent uiContent = new CUIPlayedCharCardWidget.CUIContent();

			charCardWidget.InitContent(uiContent);
			return m_DraggingCard;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (isSelf)
			{
				if (m_DraggingCard != null)
				{
					Destroy(m_DraggingCard);
					if (m_OnEndDragCallBack != null)
					{
						m_OnEndDragCallBack();
					}
				}
			}
			else
			{
				if (m_ScrollRect != null)
					m_ScrollRect.OnEndDrag(eventData);
				if (m_FixGridRect != null)
					m_FixGridRect.OnEndDrag(eventData);
			}

		}

		static public T FindInParents<T>(GameObject go) where T : Component
		{
			if (go == null) return null;
			var comp = go.GetComponent<T>();

			if (comp != null)
				return comp;

			Transform t = go.transform.parent;
			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
			return comp;
		}
	}
}
