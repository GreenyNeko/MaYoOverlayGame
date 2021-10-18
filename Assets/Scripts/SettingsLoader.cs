using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SettingsLoader : MonoBehaviour
{
    Dictionary<string, Dictionary<string, string>> preferences;
    string currentCategory;
    bool loaded;
    // Start is called before the first frame update
    void Awake()
    {
        // Empty category name by default
        currentCategory = "";
        loaded = false;
        // category,key=value
        preferences = new Dictionary<string, Dictionary<string, string>>();
        // open .ini file
        using (StreamReader streamReader = new StreamReader("preferences.ini"))
        {
            // read until end
            while(!streamReader.EndOfStream)
            {
                // get line
                string line = streamReader.ReadLine();
                // line is a category
                if (line.StartsWith("["))
                {
                    // add category
                    currentCategory = line.Substring(1, line.Length - 2);
                    preferences.Add(currentCategory, new Dictionary<string, string>());
                }
                else
                {
                    // add key,value pair to the current category
                    preferences[currentCategory][line.Substring(0, line.IndexOf('='))] = line.Substring(line.IndexOf('=') + 1);
                }
            }
            // successfully loaded
            loaded = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /**
     * Has the .ini been successfully loaded?
     */
    public bool HasLoaded()
    {
        return loaded;
    }

    /**
     * Get the loaded value to a key given the category
     * Throws errors if category or key don't exist
     */
    public string GetSetting(string category, string key)
    {
        return preferences[category][key];
    }
}
