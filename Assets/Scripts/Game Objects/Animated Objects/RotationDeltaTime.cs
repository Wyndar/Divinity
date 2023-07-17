using UnityEngine;
public class RotationDeltaTime:MonoBehaviour
{
    public int rotationSpeed;

    // Update is called once per frame
    void Update() => transform.Rotate(Vector3.forward * rotationSpeed, Space.World);
}