using GravityCollect;
using UnityEngine;

public class OnMultiTouch : MonoBehaviour
{
    [Header("Touch Settings")]
        [SerializeField] private float _moveSpeed = 5f;
        private SlimeMotion _motion;
        private Transform _transform;
        private readonly WaitForSeconds _wait = new WaitForSeconds(1f);
        private Vector2 _disdance;
    
       
        
        private void Awake() => CacheComponent();
        
        private void Update()
        {
            Move();
            OnmultiTouch();
        }
    
        
    
        
        private void OnmultiTouch()
        {
            if (Input.touchCount > 0)
            {
    
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector2 firstPosition = touch.position;
                    TouchPhase phase = touch.phase;
                    if (phase == TouchPhase.Began)
                    { 
                        //_motion.Jump();
                    }
                    else if (phase == TouchPhase.Moved)
                    {
                        Vector2 firstTouchPreviousPosition = touch.position - touch.deltaPosition; 
                        _disdance =  firstTouchPreviousPosition - firstPosition;
                        if (_disdance.x <= -30)
                        {
                            LeftRotate();
                        }
                        else if (_disdance.x >= 30)
                        {
                            RightRotate();
                        }
                    }
                    else if (phase == TouchPhase.Stationary)
                    {
                        /*//오른쪽 회전은 firstTouchPreviousPosition => firstTouch.Position  즉 이값을 확인해서 
                        Vector2 firstTouchPreviousPosition = touch.position - touch.deltaPosition; 
                        RightRotate();*/
                    }
                    else if (phase == TouchPhase.Ended)
                    {
                    }
                    else if (phase == TouchPhase.Canceled)
                    {
                    }
    
    
                }
                
            }
        }
    
        private void LeftRotate()
        {
            _transform.Rotate(new Vector3(0, 90, 0));  
            //터치 시작점 - 터치 끝나는점 거리를 일정 수 이상할 시 슬라이드로 판정해서 Rotate로 하기
        }
    
    
        private void RightRotate()
        {
            _transform.Rotate(new Vector3(0, -90, 0));
        }
        
    
        private void Move()
        {
            _motion.SetSpeed(_moveSpeed);
            Vector3 direction = new Vector3(0, 0, 1);
            
            transform.Translate(direction * _moveSpeed * Time.deltaTime);
        }
    
        private void CacheComponent()
        {
            _motion = GetComponent<SlimeMotion>();
            _transform = GetComponent<Transform>();
        }
}
