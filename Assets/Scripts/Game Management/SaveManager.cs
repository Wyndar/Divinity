using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class SaveManager:MonoBehaviour
{
    //saves card ID to json for decks
    public void SaveIDToJson(string path, List<string> data)
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.dataPath + path, json);
    }

    //loads playerdata from json
    public Player LoadPlayerFromJson(string path)
    {
        string json = File.ReadAllText(Application.dataPath + path);
        Player data = JsonConvert.DeserializeObject<Player>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads card ID from json deck
    public List<string> LoadIDFromJson(string path)
    {
        string json = File.ReadAllText(Application.dataPath + path);
        List<string> data = JsonConvert.DeserializeObject<List<string>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads json card database 
    public List<Card> LoadCardDatabase(string path)
    {
        string json = File.ReadAllText(Application.dataPath + path);
        List<Card> data = JsonConvert.DeserializeObject<List<Card>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads a list of handy tips and info from the db
    public List<ToolTipInfo> LoadToolTipInfoDatabase(string path)
    {
        string json = File.ReadAllText(Application.dataPath + path);
        List<ToolTipInfo> data = JsonConvert.DeserializeObject<List<ToolTipInfo>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }
}