using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMoveScript : MonoBehaviour
{
    [Range(1f,20f)]
    public float scrollSpeed = 1f;

    //Scroll offset value to smoothly repeat background movement
    public float scrollOffset;

    //Start position of background movement
    Vector2 Startpos;

    //Background new position
    float newPos;

    // Start is called before the first frame update
    void Start()
    {
        Startpos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        newPos = Mathf.Repeat (Time.time * - scrollSpeed, scrollOffset);
        transform.position = Startpos + Vector2.right * newPos;
    }
}
