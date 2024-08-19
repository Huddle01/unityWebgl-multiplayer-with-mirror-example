using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickMoveNavAgent : MonoBehaviour
{
    public NavMeshPlayerController LocalPlayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (LocalPlayer == null) return;

            RaycastHit hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 500))
            {
                LocalPlayer.MoveToPosition(hit.point);
            }
        }
    }
}
