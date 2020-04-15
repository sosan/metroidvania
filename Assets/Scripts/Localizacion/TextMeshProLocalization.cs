using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class LocalizationText : MonoBehaviour
{
    public string key;
    public TextMeshPro textMesh = null;

    public string value
    {

        set
        {

            textMesh.text = value;

        }

    }


    bool mStarted = false;

    void OnEnable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        if (mStarted) OnLocalize();
    }

    void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif
        mStarted = true;
        OnLocalize();
    }


    void OnLocalize()
    {



        if (string.IsNullOrEmpty(key) == false)
        {

            value = Localization.Get(key);
        }

    }
}
