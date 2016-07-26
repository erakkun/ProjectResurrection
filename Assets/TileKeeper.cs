using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileKeeper : MonoBehaviour
{
    public static Rect levelBounds;
    public static bool[,] walkInformation;
    public static bool levelSet = false;
    [SerializeField]
    public static PathFind.Grid grid;
    public static PathFind.Grid gridNoPlayer;

    void Start()
    {
        OnLevelWasLoaded(0);
    }

    public static Vector2 TranslateToPoint(Vector2 vec)
    {
        Vector2 point = vec;

        //Position: 5, 6
        //Level bounds: -10, -7, 21, 14

        //X: 0 - (-10) = 10
        //Y: -0 - (-7) = 7

        //X: -10 - (-10) = 0
        //y: -7 - (-7) = 0

        point.x = (int)point.x - (int)levelBounds.x;
        point.y = -(int)point.y - (int)levelBounds.y;

        //Debug.Log(vec + " -> " + point);

        return point;
    }

    public static Vector2 TranslateToRelative(Vector2 vec)
    {
        Vector2 point = vec;

        //X: 10 + (-10) = 0
        //Y: 7 + (-7)  = 0

        //X: 0 + (-10) = -10
        //Y: 0 + (-7) = -7

        point.x = (int)point.x + (int)levelBounds.x;
        point.y = (int)point.y + (int)levelBounds.y;

        //Debug.Log(point + " <- " + vec);

        return point;
    }

    public static bool PointIsBlock(Vector2 point, bool player = false)
    {
        return !grid.nodes[(int)point.x, (int)point.y].walkable && !player || !gridNoPlayer.nodes[(int)point.x, (int)point.y].walkable && player;
    }

    public static void SetAvailability(Vector2 point, bool tru, bool player = false)
    {
        walkInformation[(int)point.x, (int)point.y] = tru;
        grid.nodes[(int)point.x, (int)point.y].walkable = tru;
        if(!player)
        {
            gridNoPlayer.nodes[(int)point.x, (int)point.y].walkable = tru;
        }
        else
        {
            gridNoPlayer.nodes[(int)point.x, (int)point.y].walkable = true;
        }
    }

    public static bool OutOfBounds(Vector2 point)
    {
        Debug.Log("Checking " + point + " against levelBounds " + levelBounds);
        if(point.x >= levelBounds.x && point.x < levelBounds.width && point.y >= levelBounds.y && point.y < levelBounds.height)
        {
            return false;
        }

        return true;
    }

    void OnLevelWasLoaded(int level)
    {
        GameObject find = GameObject.Find("Background");
        if(find)
        {
            Transform[] seeks = find.GetComponentsInChildren<Transform>();

            int leftMost = 0, topMost = 0, bottomMost = 0, rightMost = 0;
            foreach(Transform seek in seeks)
            {
                if(seek.position.x < leftMost)
                {
                    leftMost = (int)seek.position.x;
                }
                if (seek.position.x > rightMost)
                {
                    rightMost = (int)seek.position.x;
                }
                if (seek.position.y < bottomMost)
                {
                    bottomMost = (int)seek.position.y;
                }
                if (seek.position.y > topMost)
                {
                    topMost = (int)seek.position.y;
                }
            }

            levelBounds.x = leftMost;
            levelBounds.y = -topMost;
            levelBounds.width = rightMost - leftMost + 1;
            levelBounds.height = -(bottomMost - topMost - 1);

            Debug.Log("Level bounds: " + levelBounds);

            walkInformation = new bool[(int)levelBounds.width, (int)levelBounds.height];

            foreach (Transform seek in seeks)
            {
                Vector2 point = TranslateToPoint(seek.position);

                //Debug.Log(seek.gameObject.tag + " " + seek.position + " " + point.x + " " + point.y);

                if(seek.gameObject.tag == "TileNonWalkable")
                {
                    walkInformation[(int)point.x, (int)point.y] = false;
                }
                else
                {
                    walkInformation[(int)point.x, (int)point.y] = true;
                }
            }

            grid = new PathFind.Grid((int)levelBounds.width, (int)levelBounds.height, walkInformation);
            gridNoPlayer = new PathFind.Grid((int)levelBounds.width, (int)levelBounds.height, walkInformation);

            levelSet = true;
        }
    }
}
