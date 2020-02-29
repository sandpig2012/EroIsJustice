using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sliding : MonoBehaviour
{
    public GameObject skill;
    Transform slidintransform;
    Transform slidouttransform;
    Vector3 pos;
    public float distance;
    Vector3 diriction = new Vector3(1, 0, 0);


    public void slidin(){

        distance = 0.0f;

        while (distance < 200.0f)
        {
            slidintransform = skill.GetComponent<Transform>();
            slidintransform.position += diriction;
            distance += 1.0f;
        }
    }

    public void slidout(){
        distance = 200.0f;
        while (distance > 0.0f)
        {
            slidintransform = skill.GetComponent<Transform>();
            slidintransform.position -= diriction;
            distance -= 1.0f;
        }
    }



}
