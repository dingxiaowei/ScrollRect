using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Mga
{
	public class CUIShowCardsPanel : MonoBehaviour
	{
		private List<CUIPlayedCharCardWidget> CardsList = new List<CUIPlayedCharCardWidget>();
		private List<CLocalCharInfo> roleInfo = new List<CLocalCharInfo>();
		public GameObject Obj_ScrollViewContent;
		public ScrollRect m_ScrollRect;
		public CFixGridRect m_FixGrid;
		private bool m_beginDrag = false;
		private CLocalCharInfo m_currentDrapChar;
		void Awake()
		{
			for (int i = 0; i < 20; i++)
			{
				var id = i;
				roleInfo.Add(new CLocalCharInfo() { ID = id, Name = "aladdin" });
			}
		}

		void Start()
		{
			while (CardsList.Count > roleInfo.Count)
			{
				DestroyImmediate(CardsList[0].gameObject);
				CardsList.RemoveAt(0);
			}
			StartCoroutine(createRoleCards());

		}

		private IEnumerator createRoleCards()
		{
			List<CLocalCharInfo> charInfos = new List<CLocalCharInfo>();
			charInfos.AddRange(roleInfo);
			int index = 0;
			for (int i = 0; i < charInfos.Count; i++)
			{
				_createRoleCard(charInfos[i], index++);
				if (index % 10 == 0)
					yield return null;
			}
		}

		private void _createRoleCard(CLocalCharInfo roleInfo, int index)
		{
			CUIPlayedCharCardWidget charCardWidget = null;
			if (CardsList.Count > index)
			{
				charCardWidget = CardsList[index];
			}
			else
			{
				var obj = Instantiate(Resources.Load<GameObject>("Prefab/RoleCard")) as GameObject;
				if (obj == null)
				{
					UnityEngine.Debug.LogError("有误");
					return;
				}
				obj.name = roleInfo.Name;
				
				charCardWidget = obj.GetComponent<CUIPlayedCharCardWidget>();

				if (charCardWidget == null)
				{
					UnityEngine.Debug.LogError("有误");
					return;
				}
				obj.transform.parent = Obj_ScrollViewContent.transform;
				obj.transform.localScale = Vector3.one;
				CardsList.Add(charCardWidget);
			}

			CUIPlayedCharCardWidget.CUIContent uiContent = new CUIPlayedCharCardWidget.CUIContent();
			uiContent.RoleInfo = roleInfo;
			uiContent.ScrollRectObj = m_ScrollRect;
			uiContent.FixGridRect = m_FixGrid;
			uiContent.BeginDrag = () =>
			{
				m_currentDrapChar = roleInfo;
				m_beginDrag = true;
				charCardWidget.SetState(false);
			};
			uiContent.BeginScroll = () =>
			{
				//CCommonUtility.SetActive(Obj_LeftOverLap, false);
				//CCommonUtility.SetActive(Obj_RightOverLap, false);
			};
			uiContent.EndDrag = () =>
			{
				m_currentDrapChar = null;
				m_beginDrag = false;
				charCardWidget.SetState(true);
			};
			uiContent.EndScroll = () =>
			{
				//if (!m_canShowOverLap)
				//	return;
				//CCommonUtility.SetActive(Obj_LeftOverLap, m_ScrollRect.horizontalNormalizedPosition > 0.082);
				//CCommonUtility.SetActive(Obj_RightOverLap, m_ScrollRect.horizontalNormalizedPosition < 0.98);
			};
			charCardWidget.InitContent(uiContent);
		}
	}
}
