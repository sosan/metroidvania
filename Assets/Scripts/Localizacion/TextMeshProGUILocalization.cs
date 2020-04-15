using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class TextMeshProGUILocalization : MonoBehaviour
{
	public string key;
    public TextMeshProUGUI textMesh = null;
	private bool started = false;

	private System.Diagnostics.Stopwatch stop = new System.Diagnostics.Stopwatch();

	public string value
    {

        set
        {

            textMesh.text = value;
			stop.Stop();
			print("tiempo ms=" + stop.ElapsedMilliseconds);

		}

    }

	

	private void OnEnable ()
	{


#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		

		if (started == true) OnLocalize();
	}


	private void Start ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		stop.Start();
		started = true;
		OnLocalize();
	}


	private void OnLocalize ()
	{
		value = Localization.Get(key);

	}
}
