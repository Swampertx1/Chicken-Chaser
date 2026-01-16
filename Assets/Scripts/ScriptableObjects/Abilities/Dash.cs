using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Dash")]
public class Dash : AbilityBase
{
    [SerializeField] private float distance = 1;
    [SerializeField] private float time = 0.2f;
    protected override IEnumerator Activate()
    {
        float curTime = 0;
        Vector3 start = chicken.Rb.position;
        Vector3 end = chicken.Rb.position + chicken.transform.rotation * chicken.currentMoveDirection * distance;
        var constraints = (int)chicken.Rb.constraints;
        chicken.Rb.constraints = RigidbodyConstraints.FreezePosition | (RigidbodyConstraints)constraints;
        while (curTime < time)
        {
            float percent = curTime / time;
            curTime += Time.deltaTime;
            chicken.Rb.MovePosition(Vector3.Lerp(start, end, percent));
            yield return null;


        }
        chicken.Rb.MovePosition(end);
        chicken.Rb.constraints = RigidbodyConstraints.None | (RigidbodyConstraints)constraints;
        
    }
}
