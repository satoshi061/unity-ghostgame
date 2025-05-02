using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject ghostObj; //ゴースト
    private Vector3 ghostPos; //ゴーストの位置
    private Transform _transform; //カメラの位置
    private Vector3 rotatePoint;
    private Vector3 rotateAxis; 

    void Start()
    {
        _transform = transform; //カメラの位置
        ghostObj = GameObject.FindGameObjectWithTag("Player"); //ゴースト
        ghostPos = ghostObj.transform.position; // ゴーストの位置

        rotateAxis = new Vector3(0, 1, 0);
    }

    void Update()
    {
        if (ghostObj.transform.position.z >= 0)
        {
            _transform.position = new Vector3(ghostObj.transform.position.x, ghostObj.transform.position.y, 15);
            _transform.rotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            _transform.position = new Vector3(ghostObj.transform.position.x, ghostObj.transform.position.y, -15);
            _transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
