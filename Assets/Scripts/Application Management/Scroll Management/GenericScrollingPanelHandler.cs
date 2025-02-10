using System.Collections.Generic;
using UnityEngine;

public class GenericScrollingPanelHandler<T> : MonoBehaviour
{
    [SerializeField] protected Transform content;
    [SerializeField] protected GameObject spriteHolder;
    [SerializeField] protected List<GameObject> images = new();
    [SerializeField] protected List<T> scrollItems = new();

    public void AddItem(T item) => scrollItems.Add(item);
    public void AddItemList(List<T> items) => scrollItems.AddRange(items);
    public void ClearItems() => scrollItems.Clear();

    public List<T> GetItems() => scrollItems;

    public void RemoveContent()
    {
        foreach (GameObject image in images)
            Destroy(image);
        images.Clear();
    }

    public virtual void AddContent()
    {
        foreach (T item in scrollItems)
        {
            GameObject newImage = Instantiate(spriteHolder, content);
            images.Add(newImage);
            SetupImage(newImage, item);
        }
    }

    protected virtual void SetupImage(GameObject imageObject, T item) { }
}
