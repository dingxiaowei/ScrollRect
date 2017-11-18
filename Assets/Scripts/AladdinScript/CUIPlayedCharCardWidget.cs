using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace Mga
{
	public class CLocalCharInfo
	{
		public int ID;
		public string Name;
	}

	public class CUIPlayedCharCardWidget : MonoBehaviour
	{
		public class CUIContent
		{
			public string RoleIcon = "RO_lv0";
			public ScrollRect ScrollRectObj;
			public CFixGridRect FixGridRect;
			public CLocalCharInfo RoleInfo;
			public System.Action BeginDrag = null;
			public System.Action EndDrag = null;
			public System.Action BeginScroll = null;
			public System.Action EndScroll = null;
		}
		public Image RoleIcon;
		private CUIContent m_uiContent;
		public CanvasGroup m_CanvasGroup;

		void Start()
		{

		}
		public void InitContent(CUIContent uiContent)
		{
			m_uiContent = uiContent;
			_initContent();
		}
		private void _initContent()
		{
			if (m_uiContent == null) return;

			if (RoleIcon != null)
			{
				RoleIcon.sprite = Resources.Load<Sprite>("Texture/" + m_uiContent.RoleIcon);
				var dragSrc = RoleIcon.gameObject.AddComponent<CPlayedCardOnDrag>();
				dragSrc.m_ScrollRect = m_uiContent.ScrollRectObj;
				dragSrc.m_FixGridRect = m_uiContent.FixGridRect;
				dragSrc.dragOnSurfaces = true;
				dragSrc.Init(m_uiContent.RoleInfo, m_uiContent.BeginDrag, m_uiContent.EndDrag, m_uiContent.BeginScroll, m_uiContent.EndScroll);
			}
		}

		public void SetState(bool state)
		{
			m_CanvasGroup.alpha = state ? 1 : 0;
		}
	}
}