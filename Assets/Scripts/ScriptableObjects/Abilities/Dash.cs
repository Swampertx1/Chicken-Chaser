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
        Vector3 start = _chicken.Rb.position;
        Vector3 end = _chicken.Rb.position + _chicken.transform.rotation * _chicken.currentMoveDirection * distance;
        var constraints = (int)_chicken.Rb.constraints;
        _chicken.Rb.constraints = RigidbodyConstraints.FreezePosition | (RigidbodyConstraints)constraints;
        while (curTime < time)
        {
            float percent = curTime / time;
            curTime += Time.deltaTime;
            _chicken.Rb.MovePosition(Vector3.Lerp(start, end, percent));
            yield return null;


        }
        _chicken.Rb.MovePosition(end);
        _chicken.Rb.constraints = RigidbodyConstraints.None | (RigidbodyConstraints)constraints;
        
    }
}
