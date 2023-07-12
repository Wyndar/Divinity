using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ScaleImageToFitScreen : MonoBehaviour
{
    private Image sr;

    private void Start()
    {
        sr = GetComponent<Image>();

        // world height is always camera's orthographicSize * 2
        float worldScreenHeight = Camera.main.orthographicSize * 2;

        // world width is calculated by diving world height with screen heigh
        // then multiplying it with screen width
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
        Debug.Log(worldScreenHeight +" "+ worldScreenWidth);
        // to scale the game object we divide the world screen width with the
        // size x of the sprite, and we divide the world screen height with the
        // size y of the sprite
        transform.localScale = new Vector3(
            worldScreenWidth * worldScreenHeight/10
            // sr.sprite.bounds.size.x
            ,
            worldScreenHeight * worldScreenHeight/10
            // sr.sprite.bounds.size.y
            , 1);
    }
}
