using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public class SaveManager:MonoBehaviour
{
    //saves card ID to json for decks
    public void SaveIDToJson(string path, List<CardOwnedID> data)
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + path, json);
    }

    //load card ID from json for decks
    public List<CardOwnedID> LoadIDFromJson(string path)
    {
        string json = File.ReadAllText(Application.persistentDataPath+ path);
        List<CardOwnedID> data = JsonConvert.DeserializeObject<List<CardOwnedID>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    public void SaveDecksToJson(string path, List<Deck> data)
    {
        string json = JsonConvert.SerializeObject(data);
        File.WriteAllText(Application.persistentDataPath + path, json);
    }
    public List<Deck> LoadDecksFromJson(string path)
    {
        string json = File.ReadAllText(Application.persistentDataPath + path);
        List<Deck> data = JsonConvert.DeserializeObject<List<Deck>>(json, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads playerdata from json
    public Player LoadPlayerFromJson(string path)
    {
        TextAsset json = Resources.Load<TextAsset>(path);
        Player data = JsonConvert.DeserializeObject<Player>(json.text, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads card ID from prebuilt json deck, for AI and structure decks
    public List<CardOwnedID> ReadIDFromJson(string path)
    {
        TextAsset json = Resources.Load<TextAsset>(path);
        List<CardOwnedID> data = JsonConvert.DeserializeObject<List<CardOwnedID>>(json.text, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads json card database 
    public List<Card> LoadCardDatabase(string path)
    {
        TextAsset json = Resources.Load<TextAsset>(path);
        List<Card> data = JsonConvert.DeserializeObject<List<Card>>(json.text, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }

    //loads a list of handy tips and info from the db
    public List<ToolTipInfo> LoadToolTipInfoDatabase()
    {
        TextAsset json = Resources.Load<TextAsset>("Load Data/Game Data/ToolTipsInfo");
        List<ToolTipInfo> data = JsonConvert.DeserializeObject<List<ToolTipInfo>>(json.text, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }
    public List<Deck> ReadDecksFromJson(string path)
    {
        TextAsset json = Resources.Load<TextAsset>(path);
        List<Deck> data = JsonConvert.DeserializeObject<List<Deck>>(json.text, new JsonSerializerSettings()
        {
            ContractResolver = new PrivateResolver(),
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
        return data;
    }
}