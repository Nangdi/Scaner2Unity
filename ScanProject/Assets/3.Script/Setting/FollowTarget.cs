using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;   // ��ȯ�� ������Ʈ
    public Vector3 frontOffset = new Vector3(0, 2, 5);
    public Vector3 sideOffset = new Vector3(5, 2, 0);
    public Vector3 currentOffset;
    private bool isSideView = false;
    private void Start()
    {
        currentOffset = frontOffset;
    }
    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + currentOffset;
            transform.LookAt(target);
        }
    }
    public void SwitchPointofView()
    {
        isSideView = !isSideView;

        currentOffset = isSideView ? sideOffset : frontOffset;

        // ��� �ݿ��ϰ� �ʹٸ�:
        if (target != null)
        {
            transform.position = target.position + currentOffset;
            transform.LookAt(target);
        }
    }
}
