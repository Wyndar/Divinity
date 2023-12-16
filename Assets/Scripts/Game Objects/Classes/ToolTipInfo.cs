[System.Serializable]
public class ToolTipInfo
{
    public string key;
    public string value;
    public ToolTipInfo(string keyWord, string valueWord )
    {
        key = keyWord;
        value= valueWord;
    }
}