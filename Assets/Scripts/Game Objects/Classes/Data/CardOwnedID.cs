[System.Serializable]
public class CardOwnedID
{
    public string ID { get; private set; }
    public int Count { get; private set; }

    public void SetCount(int count) => Count = count;
}
