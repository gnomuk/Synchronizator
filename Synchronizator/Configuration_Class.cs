using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class Parameter
{
    public bool Enabled { get; set; }
    public Dictionary<string, string> Keybinds { get; set; }

    public Parameter()
    {
        Keybinds = new Dictionary<string, string>();
    }
}

public class ViewModelConfiguration
{
    public Dictionary<string, Parameter> Parameters { get; set; }

    public ViewModelConfiguration()
    {
        Parameters = new Dictionary<string, Parameter>();
    }

    public void AddParameter(string parameterName, bool enabled, Dictionary<string, string> keybinds)
    {
        Parameters[parameterName] = new Parameter
        {
            Enabled = enabled,
            Keybinds = keybinds
        };
    }
    public void SaveToJson(string filePath)
    {
        var json = JsonConvert.SerializeObject(Parameters, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public void LoadFromJson(string filePath)
    {
        var json = File.ReadAllText(filePath);
        Parameters = JsonConvert.DeserializeObject<Dictionary<string, Parameter>>(json);
    }
}