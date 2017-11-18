using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceShow : MonoBehaviour
{
	EnhancelScrollView m_enhanceScrollView;
	public Transform m_ScrollView;
	void Start()
	{
		var obj = GameObject.Find("ScrollView");
		m_enhanceScrollView = obj.gameObject.AddComponent<EnhancelScrollView>();
		
		var curve1 = new AnimationCurve();
		curve1.AddKey(new Keyframe() { time = 0, value = 0 });
		curve1.AddKey(new Keyframe() { time = 0.5f, value = 1 });
		curve1.AddKey(new Keyframe() { time = 1, value = 0 });
		curve1.postWrapMode = WrapMode.Loop;
		curve1.preWrapMode = WrapMode.Loop;
		m_enhanceScrollView.scaleCurve = curve1;

		var curve2 = new AnimationCurve();
		curve2.AddKey(new Keyframe() { time = 0, value = 0 });
		curve2.AddKey(new Keyframe() { time = 1, value = 1 });
		curve2.postWrapMode = WrapMode.Loop;
		curve2.preWrapMode = WrapMode.Loop;
		m_enhanceScrollView.positionCurve = curve2;


		for (int i = 0; i < 5; i++)
		{
			var enhanceItemObj = GameObject.Instantiate(Resources.Load<GameObject>("Prefab/ImageCard"));
			var enHanceItem = enhanceItemObj.AddComponent<EnhanceItem>();
			var dragCard = enhanceItemObj.AddComponent<CDragOnCard>();
			dragCard.DragCallBack = (pos) =>
			{
				if (pos == DragPosition.Left)
					m_enhanceScrollView.OnBtnLeftClick();
				else if (pos == DragPosition.Right)
					m_enhanceScrollView.OnBtnRightClick();
			};
			enHanceItem.scrollViewItemIndex = i;
			enHanceItem.transform.SetParent(m_ScrollView);
			enHanceItem.transform.localScale = Vector3.one;
			enHanceItem.transform.localPosition = Vector3.zero;
			m_enhanceScrollView.scrollViewItems.Add(enHanceItem);
			enHanceItem.Init(i + 1);
		}
		m_enhanceScrollView.Init();
	}


}
