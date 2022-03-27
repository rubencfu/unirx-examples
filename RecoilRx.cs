using UnityEngine;
using UniRx;

public class RecoilRx : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;

    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;

    void Start()
    {
        Observable.EveryUpdate()
        .Subscribe(_ => CalcRotations());
    }

    private void CalcRotations()
    {
        targetRotation = GetTargetRotation();
        currentRotation = GetCurrentRotation();
        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    private Vector3 GetTargetRotation() => Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);

    private Vector3 GetCurrentRotation() => Vector3.Slerp(currentRotation, targetRotation, snappiness * Time.fixedDeltaTime);

    public void RecoilFire()
    {
        targetRotation += new Vector3(
            recoilX, 
            Random.Range(-recoilY, recoilY), 
            Random.Range(-recoilZ, recoilZ)
        );
    }
}
