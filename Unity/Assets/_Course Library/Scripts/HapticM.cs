using Haply.HardwareAPI.Unity;
using UnityEngine;

public class HapticController : MonoBehaviour
/*
햅틱 장치에 적용할 힘을 설정
햅틱 스레드가 초기화되면 지정된 힘을 적용하는 기능 담당.
*/
{
    [Range(-2, 2)]
    public float forceX; // X축 방향으로 적용할 힘
    [Range(-2, 2)]
    public float forceY; // Y축 방향으로 적용할 힘
    [Range(-2, 2)]
    public float forceZ; // Z축 방향으로 적용할 힘

    private void Awake()
    {
        // HapticThread 컴포넌트를 가져오기
        var hapticThread = GetComponent<HapticThread>();
        // HapticThread가 초기화되면 ForceCalculation 함수를 실행하도록 설정
        hapticThread.onInitialized.AddListener(() => hapticThread.Run(ForceCalculation));
    }

    // 해프틱 장치에 적용할 힘을 계산하는 함수
    private Vector3 ForceCalculation(in Vector3 position)
    {
        // 설정된 힘 값을 반환
        return new Vector3(forceX, forceY, forceZ);
    }
}
