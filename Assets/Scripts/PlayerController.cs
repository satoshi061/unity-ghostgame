using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    //移動速度
    [SerializeField] private float moveSpeed = 20.0f;
    [SerializeField] private float throughTime = 10.0f; //すり抜け時間
    [SerializeField] private float fadeOutTime = 1.5f; //フェードアウト時間
    [SerializeField] private Vector3 startPosition = new Vector3(-10f, 10f, 2.5f); //ステージ1のスタート位置
    [SerializeField] private Vector3 ghostSize = new Vector3(1.5f, 1.5f, 1.5f); //ゴーストのサイズ 
    private CharacterController _characterController;
    private Transform _transform;
    private Vector3 _moveVelocity;
    private InputAction _move;
    private InputAction _through; //壁すり抜け
    private RaycastHit hit;

    public GameObject cameraobj;
    private GameObject wallobj;
    private GameObject key;
    private GameObject goal;

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _transform = transform;
        var input = GetComponent<PlayerInput>();
        input.currentActionMap.Enable();
        _move = input.currentActionMap.FindAction("Move");
        _through = input.currentActionMap.FindAction("Through");
        _transform.position = startPosition;
        key = GameObject.FindGameObjectWithTag("key");
        goal = GameObject.FindGameObjectWithTag("goal");
        goal.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        //ライトに見つかる
        if (other.gameObject.tag == "light")
        {
            Vector3 fadeSize = new Vector3(0, 0, 0);
            StartCoroutine(FadeOut(ghostSize, fadeSize, fadeOutTime));
            StartCoroutine(Restart(startPosition, ghostSize));
        }
        //鍵を見つける
        if (other.gameObject.tag == "key")
        {
            goal.SetActive(true);
            Destroy(key);
        }
        //ゴール1
        if (other.gameObject.tag == "goal")
        {
            Debug.Log("ゴール");
            SceneManager.LoadSceneAsync("MainScene2");
        }
    }

    private void Update()
    {
        //壁すり抜け
        if (_through.WasPressedThisFrame())
        {
            //すり抜け先
            Vector3 targetPosition = new Vector3(_transform.position.x, _transform.position.y, -_transform.position.z);
            //Ray判定
            Physics.Linecast(_transform.position, targetPosition, out hit);
            if (hit.collider != null)
            {
                wallobj = hit.collider.gameObject; //壁のGameObject
            }
            if (wallobj.CompareTag("thinWall")) //薄い壁ならすり抜け可能
            {
                //cameraobj.GetComponent<CameraController>().StartCoroutine("rotateCamera");
                //コルーチン
                StartCoroutine(WallThrough(targetPosition, throughTime));
            }
        }

        var moveValue = _move.ReadValue<Vector2>();
        if (_transform.position.z < 0)
        {
            _moveVelocity.x = moveValue.x * moveSpeed;
        }
        else
        {
            _moveVelocity.x = -moveValue.x * moveSpeed;
        }
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
            _transform.position = Vector3.Lerp(_transform.position, targetPosition, ratio);
            if (ratio > 0.7f)
            {
                //目標の値に到達したらCoroutineを終了
                break;
            }
            yield return null;
        }
    }

    private IEnumerator FadeOut(Vector3 ghostSize, Vector3 fadeSize, float time)
    {
        var sumTime = 0f;
        while (true)
        {
            sumTime += Time.deltaTime; //Coroutineフレームから何秒経過したか
            var ratio = sumTime / time; //指定時間に対して経過した時間の割合
            _transform.localScale = Vector3.Lerp(ghostSize, fadeSize, ratio);
            if (ratio > 1.0f)
            {
                //目標の値に到達したらCoroutineを終了
                break;
            }
            yield return null;
        }
    }

    private IEnumerator Restart(Vector3 startPosition, Vector3 ghostSize)
    {
        yield return new WaitForSeconds(2.0f);
        _transform.position = startPosition;
        _transform.localScale = ghostSize;
    }

}
