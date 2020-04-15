
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;

public static class Localization
{
	public delegate byte[] LoadFunction (string path);
	public delegate void OnLocalizeNotification ();

	static public LoadFunction loadFunction;

	static public OnLocalizeNotification onLocalize;
	static public bool localizationHasBeenSet = false;
	static string[] mLanguages = null;

	static Dictionary<string, string> mOldDictionary = new Dictionary<string, string>();

	static Dictionary<string, string[]> mDictionary = new Dictionary<string, string[]>();

	
	static Dictionary<string, string> mReplacement = new Dictionary<string, string>();

	static int mLanguageIndex = -1;

	// current language
	static string mLanguage;

	static public Dictionary<string, string[]> dictionary
	{
		get
		{
			if (!localizationHasBeenSet) LoadDictionary(PlayerPrefs.GetString("Language", "English"));
			return mDictionary;
		}
		set
		{
			localizationHasBeenSet = (value != null);
			mDictionary = value;
		}
	}



    static public string[] knownLanguages
	{
		get
		{
			if (!localizationHasBeenSet) LoadDictionary(PlayerPrefs.GetString("Language", "English"));
			return mLanguages;
		}
	}


	static public string language
	{
		get
		{
			if (string.IsNullOrEmpty(mLanguage))
			{
				mLanguage = PlayerPrefs.GetString("Language", "English");
				LoadAndSelect(mLanguage);
			}
			return mLanguage;
		}
		set
		{
			if (mLanguage != value)
			{
				mLanguage = value;
				LoadAndSelect(value);
			}
		}
	}


	static bool LoadDictionary (string value)
	{
		// Try to load the Localization CSV
		byte[] bytes = null;

		if (!localizationHasBeenSet)
		{
			if (loadFunction == null)
			{
				TextAsset asset = Resources.Load<TextAsset>("Localization");
				if (asset != null) bytes = asset.bytes;
			}
			else bytes = loadFunction("Localization");
			localizationHasBeenSet = true;
		}

		if (LoadCSV(bytes)) return true;
		if (string.IsNullOrEmpty(value)) value = mLanguage;

		if (string.IsNullOrEmpty(value)) return false;

		if (loadFunction == null)
		{
			TextAsset asset = Resources.Load<TextAsset>(value);
			if (asset != null) bytes = asset.bytes;
		}
		else bytes = loadFunction(value);

		if (bytes != null)
		{
			Set(value, bytes);
			return true;
		}
		return false;
	}

	static bool LoadAndSelect (string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			if (mDictionary.Count == 0 && !LoadDictionary(value)) return false;
			if (SelectLanguage(value)) return true;
		}

		if (mOldDictionary.Count > 0) return true;

		mOldDictionary.Clear();
		mDictionary.Clear();
		if (string.IsNullOrEmpty(value)) PlayerPrefs.DeleteKey("Language");
		return false;
	}


	static public void Load (TextAsset asset)
	{
		ByteReader reader = new ByteReader(asset);
		Set(asset.name, reader.ReadDictionary());
	}


	static public void Set (string languageName, byte[] bytes)
	{
		ByteReader reader = new ByteReader(bytes);
		Set(languageName, reader.ReadDictionary());
	}


	static public void ReplaceKey (string key, string val)
	{
		if (!string.IsNullOrEmpty(val)) mReplacement[key] = val;
		else mReplacement.Remove(key);
	}


	static public void ClearReplacements () { mReplacement.Clear(); }

	static public bool LoadCSV (TextAsset asset, bool merge = false) { return LoadCSV(asset.bytes, asset, merge); }

	static public bool LoadCSV (byte[] bytes, bool merge = false) { return LoadCSV(bytes, null, merge); }

	static bool mMerging = false;

	static bool HasLanguage (string languageName)
	{
		for (int i = 0, imax = mLanguages.Length; i < imax; ++i)
			if (mLanguages[i] == languageName) return true;
		return false;
	}


	static bool LoadCSV (byte[] bytes, TextAsset asset, bool merge = false)
	{
		if (bytes == null) return false;
		ByteReader reader = new ByteReader(bytes);

		// The first line should contain "KEY", followed by languages.
		FasterListLessGarbage<string> header = reader.ReadCSV();

		// There must be at least two columns in a valid CSV file
		if (header.size < 2) return false;
		header.RemoveAt(0);

		string[] languagesToAdd = null;
		if (string.IsNullOrEmpty(mLanguage)) localizationHasBeenSet = false;

		// Clear the dictionary
		if (!localizationHasBeenSet || (!merge && !mMerging) || mLanguages == null || mLanguages.Length == 0)
		{
			mDictionary.Clear();
			mLanguages = new string[header.size];

			if (!localizationHasBeenSet)
			{
				mLanguage = PlayerPrefs.GetString("Language", header[0]);
				localizationHasBeenSet = true;
			}

			for (int i = 0; i < header.size; ++i)
			{
				mLanguages[i] = header[i];
				if (mLanguages[i] == mLanguage)
					mLanguageIndex = i;
			}
		}
		else
		{
			languagesToAdd = new string[header.size];
			for (int i = 0; i < header.size; ++i) languagesToAdd[i] = header[i];

			// Automatically resize the existing languages and add the new language to the mix
			for (int i = 0; i < header.size; ++i)
			{
				if (!HasLanguage(header[i]))
				{
					int newSize = mLanguages.Length + 1;
					System.Array.Resize(ref mLanguages, newSize);
					mLanguages[newSize - 1] = header[i];

					Dictionary<string, string[]> newDict = new Dictionary<string, string[]>();

					foreach (KeyValuePair<string, string[]> pair in mDictionary)
					{
						string[] arr = pair.Value;
						System.Array.Resize(ref arr, newSize);
						arr[newSize - 1] = arr[0];
						newDict.Add(pair.Key, arr);
					}
					mDictionary = newDict;
				}
			}
		}

		Dictionary<string, int> languageIndices = new Dictionary<string, int>();
		for (int i = 0; i < mLanguages.Length; ++i)
			languageIndices.Add(mLanguages[i], i);

		// Read the entire CSV file into memory
		for (;;)
		{
			FasterListLessGarbage<string> temp = reader.ReadCSV();
			if (temp == null || temp.size == 0) break;
			if (string.IsNullOrEmpty(temp[0])) continue;
			AddCSV(temp, languagesToAdd, languageIndices);
		}

		if (!mMerging && onLocalize != null)
		{
			mMerging = true;
			OnLocalizeNotification note = onLocalize;
			onLocalize = null;
			note();
			onLocalize = note;
			mMerging = false;
		}
		return true;
	}


	static void AddCSV (FasterListLessGarbage<string> newValues, string[] newLanguages, Dictionary<string, int> languageIndices)
	{
		if (newValues.size < 2) return;
		string key = newValues[0];
		if (string.IsNullOrEmpty(key)) return;
		string[] copy = ExtractStrings(newValues, newLanguages, languageIndices);

		if (mDictionary.ContainsKey(key))
		{
			mDictionary[key] = copy;
			if (newLanguages == null) Debug.LogWarning("Localization key '" + key + "' is already present");
		}
		else
		{
			try
			{
				mDictionary.Add(key, copy);
			}
			catch (System.Exception ex)
			{
				Debug.LogError("Unable to add '" + key + "' to the Localization dictionary.\n" + ex.Message);
			}
		}
	}


	static string[] ExtractStrings (FasterListLessGarbage<string> added, string[] newLanguages, Dictionary<string, int> languageIndices)
	{
		if (newLanguages == null)
		{
			string[] values = new string[mLanguages.Length];
			for (int i = 1, max = Mathf.Min(added.size, values.Length + 1); i < max; ++i)
				values[i - 1] = added[i];
			return values;
		}
		else
		{
			string[] values;
			string s = added[0];

			if (!mDictionary.TryGetValue(s, out values))
				values = new string[mLanguages.Length];

			for (int i = 0, imax = newLanguages.Length; i < imax; ++i)
			{
				string language = newLanguages[i];
				int index = languageIndices[language];
				values[index] = added[i + 1];
			}
			return values;
		}
	}


    static public void Broadcast(string funcName)
    {
        GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
        for (int i = 0, imax = gos.Length; i < imax; ++i) gos[i].SendMessage(funcName, SendMessageOptions.DontRequireReceiver);
    }


    static bool SelectLanguage (string language)
	{
		mLanguageIndex = -1;

		if (mDictionary.Count == 0) return false;

		for (int i = 0, imax = mLanguages.Length; i < imax; ++i)
		{
			if (mLanguages[i] == language)
			{
				mOldDictionary.Clear();
				mLanguageIndex = i;
				mLanguage = language;
				PlayerPrefs.SetString("Language", mLanguage);
				if (onLocalize != null) onLocalize();
				Broadcast("OnLocalize");
				return true;
			}
		}
		return false;
	}


	static public void Set (string languageName, Dictionary<string, string> dictionary)
	{
		mLanguage = languageName;
		PlayerPrefs.SetString("Language", mLanguage);
		mOldDictionary = dictionary;
		localizationHasBeenSet = true;
		mLanguageIndex = -1;
		mLanguages = new string[] { languageName };
		if (onLocalize != null) onLocalize();
		Broadcast("OnLocalize");

        


    }


	static public void Set (string key, string value)
	{
		if (mOldDictionary.ContainsKey(key)) mOldDictionary[key] = value;
		else mOldDictionary.Add(key, value);
	}


	static public string Get (string key)
	{
		//if (string.IsNullOrEmpty(key)) return null;

		if (!localizationHasBeenSet) LoadDictionary(PlayerPrefs.GetString("Language", "English"));

		if (mLanguages is null)
		{
			Debug.LogError("No localization data present");
			return null;
		}

		string lang = language;

		if (mLanguageIndex == -1)
		{
			for (int i = 0; i < mLanguages.Length; ++i)
			{
				if (mLanguages[i] == lang)
				{
					mLanguageIndex = i;
					break;
				}
			}
		}

		if (mLanguageIndex == -1)
		{
			mLanguageIndex = 0;
			mLanguage = mLanguages[0];
			Debug.LogWarning("Language not found: " + lang);
		}

		string val;
		string[] vals;

        
		

		if (mReplacement.TryGetValue(key, out val)) return val;

		if (mLanguageIndex != -1 && mDictionary.TryGetValue(key, out vals))
		{
			if (mLanguageIndex < vals.Length)
			{
				string s = vals[mLanguageIndex];
				if (string.IsNullOrEmpty(s)) s = vals[0];
				return s;
			}
			return vals[0];
		}
		if (mOldDictionary.TryGetValue(key, out val)) return val;

#if UNITY_EDITOR
		Debug.LogWarning("Localization key not found: '" + key + "' for language " + lang );
#endif
		return key;
	}


	static public string Format (string key, params object[] parameters) { return string.Format(Get(key), parameters); }

	//[System.Obsolete("Localization is now always active. You no longer need to check this property.")]
	//static public bool isActive { get { return true; } }

	//[System.Obsolete("Use Localization.Get instead")]
	//static public string Localize (string key) { return Get(key); }


	static public bool Exists (string key)
	{
		// Ensure we have a language to work with
		if (!localizationHasBeenSet) language = PlayerPrefs.GetString("Language", "English");

#if UNITY_IPHONE || UNITY_ANDROID
		string mobKey = key + " Mobile";
		if (mDictionary.ContainsKey(mobKey)) return true;
		else if (mOldDictionary.ContainsKey(mobKey)) return true;
#endif
		return mDictionary.ContainsKey(key) || mOldDictionary.ContainsKey(key);
	}

	

	static public void Set (string language, string key, string text)
	{
		string[] kl = knownLanguages;
		
		if (kl == null)
		{
			mLanguages = new string[] { language };
			kl = mLanguages;
		}

		for (int i = 0, imax = kl.Length; i < imax; ++i)
		{
			if (kl[i] == language)
			{
				string[] vals;

				if (!mDictionary.TryGetValue(key, out vals))
				{
					vals = new string[kl.Length];
					mDictionary[key] = vals;
					vals[0] = text;
				}

				vals[i] = text;
				return;
			}
		}

		int newSize = mLanguages.Length + 1;
		System.Array.Resize(ref mLanguages, newSize);
		mLanguages[newSize - 1] = language;

		Dictionary<string, string[]> newDict = new Dictionary<string, string[]>();

		foreach (KeyValuePair<string, string[]> pair in mDictionary)
		{
			string[] arr = pair.Value;

			System.Array.Resize(ref arr, newSize);
			arr[newSize - 1] = arr[0];
			newDict.Add(pair.Key, arr);
		}
		mDictionary = newDict;

		string[] values;

		if (!mDictionary.TryGetValue(key, out values))
		{
			values = new string[kl.Length];
			mDictionary[key] = values;
			values[0] = text;
		}
		values[newSize - 1] = text;
	}
}
