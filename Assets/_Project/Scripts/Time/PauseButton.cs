using Terminus.Game.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace BrightFish
{
	public class PauseButton : MonoBehaviour
	{
		private Toggle m_MenuToggle;
		private float m_TimeScaleRef = 1f;
		private float m_VolumeRef = 1f;
		private bool m_Paused;

		//----------------------------------------------------------------

		public void OnMenuStatusChange()
		{
			if (!m_MenuToggle.isOn && !m_Paused)
			{
				MenuOn();
			}
			else if (m_MenuToggle.isOn && m_Paused)
			{
				MenuOff();
			}
		}

		//----------------------------------------------------------------

		private void Awake()
		{
			m_MenuToggle = GetComponent<Toggle>();
		}

		private void MenuOn()
		{
			m_TimeScaleRef = Time.timeScale;
			Time.timeScale = 0f;

			m_VolumeRef = AudioListener.volume;
			AudioListener.volume = 0f;

			m_Paused = true;

			MessageBus.OnGamePause.Send(m_Paused);

			Debug.Log("Pause");
		}

		private void MenuOff()
		{
			Time.timeScale = m_TimeScaleRef;
			AudioListener.volume = m_VolumeRef;
			m_Paused = false;

			MessageBus.OnGamePause.Send(m_Paused);

			Debug.Log("Play");
		}

#if !MOBILE_INPUT
	void Update()
	{
		if(Input.GetKeyUp(KeyCode.Escape))
		{
		    m_MenuToggle.isOn = !m_MenuToggle.isOn;
            Cursor.visible = m_MenuToggle.isOn;//force the cursor visible if anythign had hidden it
		}
	}
#endif

	}
}