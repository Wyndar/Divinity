using UnityEngine;

public class AttackArrowMovement : MonoBehaviour
{
    public Transform targetPosition;
    public float speed;

    // Update is called once per frame
    void Update()
    {
        if (targetPosition == null)
            return;
        Quaternion lookRot = Quaternion.LookRotation(transform.position, targetPosition.position);
        Quaternion.RotateTowards(transform.rotation, lookRot, 360f);
        if (Vector3.Distance(transform.position, targetPosition.position) > 0)
            transform.Translate(speed * Time.deltaTime * targetPosition.position);
        else
            Destroy(gameObject);
    }
}
