using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DragPosition
{
	Left,
	Right,
	Up,
	Down,
}

[RequireComponent(typeof(Image))]
public class CDragOnCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public bool dragOnSurfaces = true;
	public ScrollRect m_ScrollRect = null;
	public CFixGridRect m_FixGridRect = null;
	private RectTransform m_DraggingPlane;

	public bool isVertical = false;
	private bool isSelf = false;
	private DragPosition m_dragPosition = DragPosition.Left;

	public System.Action<DragPosition> DragCallBack = null;

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
			if(touchDeltaPosition.y > 0)
			{
				UnityEngine.Debug.Log("上拖");
				m_dragPosition = DragPosition.Up;
			}
			else
			{
				UnityEngine.Debug.Log("下拖");
				m_dragPosition = DragPosition.Down;
			}

			if (Mathf.Abs(touchDeltaPosition.x) > Mathf.Abs(touchDeltaPosition.y))
			{
				isSelf = true;
				var canvas = FindInParents<Canvas>(gameObject);
				if (canvas == null)
					return;

				if (dragOnSurfaces)
					m_DraggingPlane = transform as RectTransform;
				else
					m_DraggingPlane = canvas.transform as RectTransform;

			}
			else
			{
				isSelf = false;
				if (m_ScrollRect != null)
					m_ScrollRect.OnBeginDrag(eventData);
			}
		}
		else //水平
		{
			if (touchDeltaPosition.x > 0)
			{
				UnityEngine.Debug.Log("右移");
				m_dragPosition = DragPosition.Right;
			}
			else
			{
				UnityEngine.Debug.Log("左移");
				m_dragPosition = DragPosition.Left;
			}

			if (Mathf.Abs(touchDeltaPosition.x) < Mathf.Abs(touchDeltaPosition.y))
			{
				isSelf = true;
				var canvas = FindInParents<Canvas>(gameObject);
				if (canvas == null)
					return;

				if (dragOnSurfaces)
					m_DraggingPlane = transform as RectTransform;
				else
					m_DraggingPlane = canvas.transform as RectTransform;
			}
			else
			{
				isSelf = false;
				if (m_ScrollRect != null)
					m_ScrollRect.OnBeginDrag(eventData);
			}
		}
	}

	public void OnDrag(PointerEventData data)
	{
		if (isSelf)
		{

		}
		else
		{
			if (m_ScrollRect != null)
				m_ScrollRect.OnDrag(data);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (isSelf)
		{
			
		}
		else
		{
			if (m_ScrollRect != null)
				m_ScrollRect.OnEndDrag(eventData);
			if (m_FixGridRect != null)
				m_FixGridRect.OnEndDrag(eventData);
		}

		if (DragCallBack != null)
			DragCallBack(m_dragPosition);
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
