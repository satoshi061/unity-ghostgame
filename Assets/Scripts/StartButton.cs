using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class StartButton : MonoBehaviour
{
    //移動速度
    [SerializeField] private float moveSpeed = 20.0f;
    [SerializeField] private float throughTime = 2.0f; //すり抜け時間
    [SerializeField] private Vector3 startPosition = new Vector3(-50f, -30f, 70f); //ステージ1のスタート位置
    [SerializeField] private Vector3 ghostSize = new Vector3(20f, 20f, 20f); //ゴーストのサイズ 
    private CharacterController _characterController;
    private Transform _transform;
    private Vector3 _moveVelocity;
    private InputAction _move;
    private InputAction _through; //壁すdり抜け
    private RaycastHit hit;
    private Vector3 targetPosition;

    private GameObject wallobj;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        var input = GetComponent<PlayerInput>();
        input.currentActionMap.Enable();
        _move = input.currentActionMap.FindAction("Move");
        _through = input.currentActionMap.FindAction("Through");
        _transform.position = startPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        //スタート
        if (other.gameObject.tag == "start")
        {
            Debug.Log("スタート");
            SceneManager.LoadSceneAsync("MainScene1");
        }
    }

    private void Update()
    {
        var moveValue = _move.ReadValue<Vector2>();

        //壁すり抜け
        if (_through.WasPressedThisFrame())
        {
            //すり抜け先
            if (moveValue.x >= 0)
            {
                targetPosition = new Vector3(_transform.position.x + 40, _transform.position.y, _transform.position.z);
            }
            else
            {
                targetPosition = new Vector3(_transform.position.x - 40, _transform.position.y, _transform.position.z);
            }
            //Ray判定
            Physics.Linecast(_transform.position, targetPosition, out hit);
            if (hit.collider != null)
            {
                wallobj = hit.collider.gameObject; //壁のGameObject
            }
            if (wallobj.CompareTag("thinWall")) //薄い壁ならすり抜け可能
            {
                //コルーチン
                StartCoroutine(WallThrough(targetPosition, throughTime));
            }
        }

        _moveVelocity.x = moveValue.x * moveSpeed;
        _moveVelocity.y = moveValue.y * moveSpeed;
        //移動方向を向く
        if (_moveVelocity.x != 0)
        {
            _transform.LookAt(_transform.position + new Vector3(_moveVelocity.x, 0, 0));
        }
        else
        {
            _transform.LookAt(_transform.position + new Vector3(0, 0, 0));
        }
        //オブジェクトを動かす
        _characterController.Move(_moveVelocity * Time.deltaTime);
    }

    private IEnumerator WallThrough(Vector3 targetPosition, float time)
    {
        var sumTime = 0f;
        while (true)
        {
            sumTime += Time.deltaTime; //Coroutineフレームから何秒経過したか
            var ratio = sumTime / time; //指定時間に対して経過した時間の割合
            _transform.SetPositionAndRotation(
                Vector3.Lerp(_transform.position, targetPosition, ratio),
                _transform.rotation);
            Debug.Log(ratio);
            if (ratio > 0.2f)
            {
                //目標の値に到達したらCoroutineを終了
                break;
            }
            yield return null;
        }
    }
}

