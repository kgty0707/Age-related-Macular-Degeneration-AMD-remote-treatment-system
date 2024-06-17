using Haply.HardwareAPI.Unity;
using UnityEngine;
using TMPro; // TextMeshPro 네임스페이스 추가

public class CollisionDetector : MonoBehaviour
/*
충돌 감지 및 충돌 시 response 처리
특정 태그를 가진 객체와 충돌 시 색상을 변경, 사운드 재생, 충돌 횟수 기록.
*/
{
    public bool isColliding = false; // 충돌 여부를 나타내는 플래그
    public Vector3 CollisionPoint { get; private set; } // 충돌 지점을 저장
    public float HeightFactor { get; private set; } // 접근 가능한 높이 계수
    public float penetrationThreshold = -0.02f; // y축 방향 임계값
    public string CollidingObjectTag { get; private set; } // 현재 충돌 중인 객체의 태그를 저장
    public GameObject CollidingObject { get; private set; } // 현재 충돌 중인 객체를 저장
    public Vector3 EntryDirection { get; private set; }  // 충돌의 진입 방향을 저장
    private Color originalColor; // 객체의 원래 색상을 저장
    private Renderer collidingObjectRenderer; // 충돌 객체의 렌더러 컴포넌트
    public int count = 0; // "sphere"가 아닌 객체와의 충돌 횟수를 카운트

    public AudioClip sphereSound; // Sphere 사운드 클립
    public AudioClip eyeSound; // Eye 사운드 클립
    private AudioSource audioSource; // 오디오 소스 컴포넌트
    public TextMeshProUGUI countText; // 카운트를 표시할 UI 텍스트

    private bool sphereCollided = false; // Sphere가 충돌했는지 확인하는 플래그
    private bool eyeCollided = false; // Eye가 충돌했는지 확인하는 플래그
    public GameObject bloodSprayFX; // 피 분수 효과 오브젝트
    public GameObject bloodSprayFX2; // 피 분수 효과 오브젝트 (다른 모양)

    private void Start()
    {
        audioSource = GetComponent<AudioSource>(); // AudioSource 컴포넌트 가져오기
        UpdateCountText(); // 초기 카운트 텍스트 업데이트
        bloodSprayFX.SetActive(false); // 시작 시 비활성화되도록 설정
        bloodSprayFX2.SetActive(false); // 시작 시 비활성화되도록 설정
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger started with " + other.name); // 어떤 오브젝트랑 충돌했는지 파악용 로그
        isColliding = true;
        CollidingObject = other.gameObject;
        CollisionPoint = other.ClosestPoint(transform.position); // 트리거 콜라이더의 가장 가까운 지점 가져오기
        EntryDirection = (CollisionPoint - transform.position).normalized; // 진입 방향 계산 및 저장
        CollidingObjectTag = other.tag;
        UpdateHeightFactor(CollisionPoint.y); // 충돌 지점을 기준으로 HeightFactor 계산

        collidingObjectRenderer = CollidingObject.GetComponent<Renderer>();
        if (collidingObjectRenderer != null)
        {
            if (other.CompareTag("sphere")) // 목표점에 닿으면 초록색으로 변하게 색 지정 
            {
                if (!sphereCollided)
                {
                    if (collidingObjectRenderer.material.color != Color.green) 
                    {
                        collidingObjectRenderer.material.color = Color.green;
                    }
                    if (!audioSource.isPlaying || audioSource.clip != sphereSound)
                    {
                        audioSource.clip = sphereSound;
                        audioSource.Play();
                    }
                    sphereCollided = true; // 플래그 설정
                }
            }
            else if (other.CompareTag("eye")) // 목표점 이외의 eye와 충돌했을 경우 삐빅 소리내고 틀린 횟수 기록 
            {
                if (!eyeCollided)
                {
                    count++;
                    UpdateCountText();
                    if (!audioSource.isPlaying || audioSource.clip != eyeSound)
                    {
                        audioSource.clip = eyeSound;
                        audioSource.Play();
                    }
                }
                if (count >= 20)
                {
                    ChangeEyeObjectsColorToRed();
                    bloodSprayFX.SetActive(true); // 피 분수 효과 활성화
                    bloodSprayFX2.SetActive(true); // 피 분수 효과 활성화
                }
                eyeCollided = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger ended with " + other.name);
        if (other.tag == CollidingObjectTag)
        {
            isColliding = false;
            CollidingObject = null; // 트리거 종료 시 충돌 객체 지우기
            CollidingObjectTag = null; // 트리거 종료 시 태그 지우기

            if (other.CompareTag("sphere"))
            {
                sphereCollided = false;
                collidingObjectRenderer.material.color = Color.white; // 트리거 종료 시 원래 색상으로 복원
                collidingObjectRenderer = null; 
            }
            else if (other.CompareTag("eye"))
            {
                eyeCollided = false;
            }
        }
        UpdateHeightFactor(transform.position.y); // 현재 위치를 기준으로 HeightFactor 재계산
    }

    private void UpdateHeightFactor(float yPosition)
    {
        // 주어진 y 위치를 사용하여 HeightFactor 계산
        HeightFactor = Mathf.Clamp01((yPosition - penetrationThreshold) / -penetrationThreshold);
    }

    private void UpdateCountText()
    {
        countText.text = $"틀린 횟수: {count}회"; // 현재 카운트를 표시하도록 텍스트 업데이트
    }

    private void ChangeEyeObjectsColorToRed()
    {
        GameObject[] eyeObjects = GameObject.FindGameObjectsWithTag("eye");
        foreach (GameObject eyeObject in eyeObjects)
        {
            Renderer renderer = eyeObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (renderer.material.color != Color.red) // 이미 빨간색이 아닌 경우에만 색상 변경
                {
                    renderer.material.color = Color.red;
                }
            }
        }
    }
}
