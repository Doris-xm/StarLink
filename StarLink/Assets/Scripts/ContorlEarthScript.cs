using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SatelliteInspector.Scripts.PrefabScripts{
public class ContorlEarthScript : MonoBehaviour
{   
    public GameObject SatInspector3D;

    //实现GameObject随鼠标拖动3D旋转
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //实现GameObject随鼠标拖动3D旋转
        // if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftCommand))
        // {
        //     float x = Input.GetAxis("Mouse X");
        //     float y = Input.GetAxis("Mouse Y");
        //     SatInspector3D.transform.Rotate(new Vector3(0, -x, 0), Space.World);
        //     SatInspector3D.transform.Rotate(new Vector3(-y, 0, 0), Space.Self);
        // }

    }
}
}
