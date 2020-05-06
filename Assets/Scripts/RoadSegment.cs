using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadSegment : MonoBehaviour
{
    public Transform BonusParent;
    public Transform ObstacleParent;

    public void ClearContents()
    {
        for (int i = 0; i < ObstacleParent.childCount; i++)
        {
            Destroy(ObstacleParent.GetChild(i).gameObject);
        }
		
        for (int i = 0; i < BonusParent.childCount; i++)
        {
            Destroy(BonusParent.GetChild(i).gameObject);
        }
    }

    private void Update()
    {
        var extent = GameManager.RT.ScreenExtentsY;
        if (transform.position.y < extent * -1.0f)
        {
            // Out of screen
            PlaceSegment();
            SetupSegment();
        }
		
        transform.Translate(Vector3.down * GameManager.RT.MovementSpeed);
    }

    private void SetupSegment()
    {
        ClearContents();
        GameManager.RT.SetupSegment(this);
    }

    private void PlaceSegment()
    {
        var other = GameManager.RT.GetOtherSegment(this);
        var newPos = transform.position;
        newPos.y = other.transform.position.y + (GameManager.RT.ScreenExtentsY * 2.0f);
        transform.position = newPos;
    }
}
