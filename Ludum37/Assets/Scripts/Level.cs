using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;


public class Level : MonoBehaviour {

    public GameObject WallPrefab;
    public GameObject SquarePrefab;
    public GameObject Player;

	// Use this for initialization
	void Start () {

        InitializeLevel();
        CreateSquarePrefabs();
        CreateWalls();
	}

    void CreateSquarePrefabs()
    {
        // Say, 40 of them
        for (int i = 0; i < 40; i++)
        {
            GameObject go = GameObject.Instantiate(SquarePrefab);
            go.SetActive(false);
            allSquares.Add(go);
        }
    }

    void Update()
    {
        pushTimer += Time.deltaTime;

        if (pushTimer >= TimeBeforePush)
        {
            if (mainWallLink != null)
            {
                // Check if the delta position is bigger
                Vector3 delta = mainWallLink.WallBody.transform.position - mainWallLink.WallBody.GetComponent<InnerWall>().OriginalPosition;

                if (delta.magnitude >= MinOffsetToPush)
                {
                    // Time to push! If possible
                    if (TryToPush(mainWallLink))
                    {
                        // Success. Time to regen!!!
                        CreateWalls();
                    }
                }

            }
        }
    }

    void ClearInvalid()
    {
        Array.Clear(lastInvalid, 0, lastInvalid.Length);
        for (int i = 0; i < allSquares.Count; i++)
        {
            allSquares[i].SetActive(false);
        }
    }

    void SetInvalid()
    {
        // Is it different?
        bool different = false;
        for (int i = 0; !different && (i < filled.Length); i++)
        {
            different = (filled[i] != lastInvalid[i]);
        }
        if (different)
        {
            Array.Copy(filled, lastInvalid, filled.Length);
        }

        int squareIndex = 0;
        for (int i = 0; i < lastInvalid.Length; i++)
        {
            int x = i % mapSize;
            int y = i / mapSize;
            if (lastInvalid[i])
            {
                float oldZ = allSquares[squareIndex].transform.position.z;
                allSquares[squareIndex].transform.position = new Vector3((float)x + 0.5f, (float)y + 0.5f, oldZ);
                allSquares[squareIndex].SetActive(true);
                squareIndex++;
            }
        }
        for (int i = squareIndex; i < allSquares.Count; i++)
        {
            allSquares[i].SetActive(false);
        }
    }

    bool TryToPush(WallScript sourceWall)
    {
        int xSelected = sourceWall.X;
        int ySelected = sourceWall.Y;
        int moveX = 0;
        int moveY = 0;
        switch (sourceWall.Direction)
        {
            case 0: // down
                moveY = -1;
                break;
            case 1:
                moveX = 1;
                break;
            case 2:
                moveY = 1;
                break;
            case 3:
                moveX = -1;
                break;
        }

        // Has to be empty in front of us
        if (IsEdge(xSelected, ySelected, moveX, moveY))
        {
            // Then we need to find how wide the wall is, and try to move it all.
            // Check left and right for edges
            int edgeDX = moveY;
            int edgeDY = -moveX;
            int min = 0;
            int xEdge = xSelected - edgeDX;
            int yEdge = ySelected - edgeDY;
            while (IsEdge(xEdge, yEdge, moveX, moveY))
            {
                min--;
                xEdge -= edgeDX;
                yEdge -= edgeDY;
            }
            int max = 0;
            xEdge = xSelected + edgeDX;
            yEdge = ySelected + edgeDY;
            while (IsEdge(xEdge, yEdge, moveX, moveY))
            {
                max++;
                xEdge += edgeDX;
                yEdge += edgeDY;
            }

            // Then make a copy of the array so we can backup
            Array.Copy(filled, backup, filled.Length);

            // Then slide all these rows
            for (int i = min; i <= max; i++)
            {
                int x = xSelected + i * edgeDX;
                int y = ySelected + i * edgeDY;
                SlideRow(x, y, moveX, moveY);
            }

            // Then check for consistency.
            if (!IsOneRoom())
            {
                // Restore if not
                // Then make a copy of the array so we can backup
                // But first, store the invalid
                SetInvalid();
                Array.Copy(backup, filled, filled.Length);
                return false;
            }

            // Now push the player ahead a bit
            Player.transform.position += new Vector3(moveX, moveY, 0) * (1f - MinOffsetToPush) * 0.3f;

            // And slow him down - this does not seem to work.???
            // setting velocity directly no work too
            Player.GetComponent<Rigidbody2D>().AddForce(-Player.GetComponent<Rigidbody2D>().velocity, ForceMode2D.Impulse);
        }
        ClearInvalid();
        return true;
    }

    // Update is called once per frame
    void FixedUpdate () {

        foreach (KeyValuePair<WallScript, List<WallScript>> wallLink in wallLinks)
        {
            Vector3 delta = wallLink.Key.WallBody.transform.position - wallLink.Key.WallBody.GetComponent<InnerWall>().OriginalPosition;
            // Oh of course... it's always gonna be zero.

            foreach (WallScript dependentWall in wallLink.Value)
            {
                dependentWall.WallBody.transform.position = dependentWall.WallBody.GetComponent<InnerWall>().OriginalPosition + delta;
            }

        }

    }

    void InitializeLevel()
    {
        bool initializedPlayer = false;
        for (int i = 0; i < things.Length; i++)
        {
            if (things[i] == 1)
            {
                filled[i] = true;
                if (!initializedPlayer)
                {
                    initializedPlayer = true;
                    Player.transform.position = new Vector3((float)(i % mapSize) + 0.5f, (float)(i / mapSize) + 0.5f);
                }
            }
        }
    }

    private int[] things = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,1,1,0,0,
            0,0,0,1,1,1,1,0,
            0,0,0,1,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
    };
    private bool[] filled = new bool[mapSize * mapSize];
    private bool[] backup = new bool[mapSize * mapSize];
    private bool[] lastInvalid = new bool[mapSize * mapSize];
    private List<GameObject> allSquares = new List<GameObject>();


    private const int mapSize = 8;

    private Dictionary<WallScript, List<WallScript>> wallLinks = new Dictionary<WallScript, List<WallScript>>();
    private WallScript mainWallLink = null;

    bool GetAt(int x, int y)
    {
        return filled[x + y * mapSize];
    }
    bool GetAtBounded(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapSize || y >= mapSize)
        {
            return false;
        }
        return filled[x + y * mapSize];
    }

    private List<WallScript> allWalls = new List<WallScript>();

    private WallScript FindWall(int x, int y, int direction)
    {
        foreach (WallScript testWall in allWalls)
        {
            if ((testWall.Direction == direction) &&
                testWall.X == x &&
                testWall.Y == y)

            {
                return testWall;
            }
        }
        return null;
    }

    private void UnlinkWall(WallScript wall)
    {
        // Ensure this guy is no longer linked anywhere
        foreach (KeyValuePair<WallScript, List<WallScript>> wallLink in wallLinks)
        {
            if (wallLink.Value.Contains(wall))
            {
                wallLink.Value.Remove(wall);
            }
        }

        if (wallLinks.ContainsKey(wall))
        {
            wallLinks.Remove(wall);
        }
    }

    // Once we've been pushing on wall for a bit, then 

    private WallScript FindRearWall(WallScript wall)
    {
        int xOrig = wall.X;
        int yOrig = wall.Y;
        int opposingDirection = (wall.Direction + 2) % 4;
        switch (wall.Direction)
        {
            case 0:
                // look above (e.g. + y)
                for (int y = yOrig; y <= mapSize; y++)
                {
                    WallScript testWall = FindWall(xOrig, y, opposingDirection);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;

           case 3:
                // look right (+x)
                for (int x = xOrig; x <= mapSize; x++)
                {
                    WallScript testWall = FindWall(x, yOrig, opposingDirection);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;
            case 2:
                // look below (e.g. - y)
                for (int y = yOrig; y >= 0; y--)
                {
                    WallScript testWall = FindWall(xOrig, y, opposingDirection);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;
            case 1:
                for (int x = xOrig; x >= 0; x--)
                {
                    WallScript testWall = FindWall(x, yOrig, opposingDirection);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;
        }
        Debug.Log("uh oh");
        return null;
    }

    public float TimeBeforePush = 0.5f;
    public float MinOffsetToPush = 0.3f;

    private float pushTimer = 0f;

    public void OnStartPushWall(WallScript wall)
    {
        pushTimer = 0f;
        // TODO!
        // Ignore this completely if there is still another wall we're tracking!

        wall.WallBody.GetComponent<SpringJoint2D>().enabled = true;

        UnlinkWall(wall);

        // Find other adjacent walls, and have them sync'd
        int xOrig = wall.X;
        int yOrig = wall.Y;
        int direction = wall.Direction;
        List<WallScript> dependentWalls = new List<WallScript>();
        dependentWalls.Add(FindRearWall(wall));
        if ((direction % 2) == 0)
        {
            // Horizontal walls... must be on same y
            for (int x = xOrig + 1; x <= mapSize; x++)
            {
                WallScript testWall = FindWall(x, yOrig, direction);
                if (testWall != null)
                {
                    dependentWalls.Add(testWall);
                    dependentWalls.Add(FindRearWall(testWall));
                }
                else
                {
                    break;
                }
            }
            for (int x = xOrig - 1; x >= 0; x--)
            {
                WallScript testWall = FindWall(x, yOrig, direction);
                if (testWall != null)
                {
                    dependentWalls.Add(testWall);
                    dependentWalls.Add(FindRearWall(testWall));

                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            // Vertical walls... must be on same x
            for (int y = yOrig + 1; y <= mapSize; y++)
            {
                WallScript testWall = FindWall(xOrig, y, direction);
                if (testWall != null)
                {
                    dependentWalls.Add(testWall);
                    dependentWalls.Add(FindRearWall(testWall));

                }
                else
                {
                    break;
                }
            }
            for (int y = yOrig - 1; y >= 0; y--)
            {
                WallScript testWall = FindWall(xOrig, y, direction);
                if (testWall != null)
                {
                    dependentWalls.Add(testWall);
                    dependentWalls.Add(FindRearWall(testWall));

                }
                else
                {
                    break;
                }
            }
        }
        
        foreach (WallScript dependentWall in dependentWalls)
        {
            UnlinkWall(dependentWall);
            // Make them kinematic I guess? Definitely disable their springs
            dependentWall.WallBody.GetComponent<SpringJoint2D>().enabled = false;
            // And since we'll be setting the position directly:
            dependentWall.WallBody.GetComponent<Rigidbody2D>().isKinematic = false;
        }

        wallLinks[wall] = dependentWalls;
        mainWallLink = wall;

        foreach (WallScript aWall in allWalls)
        {
            if ((aWall != wall) && !dependentWalls.Contains(aWall))
            {
                aWall.WallBody.GetComponent<InnerWall>().SetColor(null);
            }
            else
            {
                aWall.WallBody.GetComponent<InnerWall>().SetColor(Color.green);
            }
        }
    }

    public void OnStopPushWall(WallScript wall)
    {
        if (wallLinks.ContainsKey(wall))
        {
            // Uncolor these
            wall.WallBody.GetComponent<InnerWall>().SetColor(null);
            foreach (WallScript aWall in wallLinks[wall])
            {
                aWall.WallBody.GetComponent<InnerWall>().SetColor(null);
            }
            ClearInvalid();
        }
        // pushTimer = 0f;
        // Maybe???

        // Actually, we want to leave it enabled so it "returns to base"
        // We'll only disable it when it's chained to other walls.
        // wall.GetComponent<SpringJoint2D>().enabled = false;
    }

    private void SetUpWall(GameObject wall, int x, int y, int direction, bool horizontal)
    {
        wall.transform.position = new Vector3((float)x + 0.5f, (float)y + 0.5f);
        wall.transform.rotation = Quaternion.AngleAxis(direction * 90f, new Vector3(0, 0, 1));
        Rigidbody2D wallBody = wall.GetComponent<WallScript>().WallBody;
        if (horizontal)
        {
            wallBody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }
        wall.GetComponent<WallScript>().SetLevel(this, x, y, direction);
        allWalls.Add(wall.GetComponent<WallScript>());
    }

    // Construct things
    // Do we do this every time? I think so.
    private void CreateWalls()
    {
        foreach (WallScript wall in allWalls)
        {
            GameObject.Destroy(wall.gameObject);
        }
        allWalls.Clear();
        wallLinks.Clear();
        mainWallLink = null;

        // For now we'll create ones for each intersection
        // (might want to join them later)
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (GetAtBounded(x, y))
                {
                    if (!GetAtBounded(x, y - 1))
                    {
                        // A wall on bottom
                        GameObject wall = GameObject.Instantiate(WallPrefab);
                        SetUpWall(wall, x, y, 0, false);
                    }
                    if (!GetAtBounded(x, y + 1))
                    {
                        // A wall on top
                        GameObject wall = GameObject.Instantiate(WallPrefab);
                        SetUpWall(wall, x, y, 2, false);
                    }
                    if (!GetAtBounded(x - 1, y))
                    {
                        // A wall on left
                        GameObject wall = GameObject.Instantiate(WallPrefab);
                        SetUpWall(wall, x, y, 3, true);
                    }
                    if (!GetAtBounded(x + 1, y))
                    {
                        // A wall on right
                        GameObject wall = GameObject.Instantiate(WallPrefab);
                        SetUpWall(wall, x, y, 1, true);
                    }
                }
            }
        }
    }

    // tile map stuff

    private void SlideRow(int xStart, int yStart, int moveX, int moveY)
    {
        // Slide them over. For now we won't check if it's ok.
        if (xStart < mapSize && yStart < mapSize)
        {
            // TODO: bounds checks
            int xMin = xStart;
            int yMin = yStart;
            int min = 0;
            while (filled[xMin + yMin * mapSize])
            {
                xMin -= moveX;
                yMin -= moveY;
                min--;
            }
            int xMax = xStart;
            int yMax = yStart;
            int max = 0;
            while (filled[xMax + yMax * mapSize])
            {
                xMax += moveX;
                yMax += moveY;
                max++;
            }

            min++;
            filled[xStart + min * moveX + (yStart + min * moveY) * mapSize] = false;
            min++;
            for (int i = min; i <= max; i++)
            {
                filled[xStart + i * moveX + (yStart + i * moveY) * mapSize] = true;
            }
        }

    }

    bool GetAtFlood(int x, int y)
    {
        return flood[x + y * mapSize];
    }

    // Is the current one filled, and the next one not.
    bool IsEdge(int x, int y, int moveX, int moveY)
    {
        // TODO: Add blockers.
        return GetAt(x, y) && !GetAt(x + moveX, y + moveY);
    }

    bool[] flood = new bool[mapSize * mapSize];

    List<Point> floodStack = new List<Point>();
    void FloodFill(int x, int y)
    {
        floodStack.Clear();
        floodStack.Add(new Point(x, y));
        while (floodStack.Count > 0)
        {
            Point point = floodStack[floodStack.Count - 1];
            floodStack.RemoveAt(floodStack.Count - 1);
            if (!GetAtFlood(point.X, point.Y) && GetAt(point.X, point.Y)) // not yet filled, but is part of room
            {
                flood[point.X + point.Y * mapSize] = true; // filled

                if (point.X > 0)
                {
                    floodStack.Add(new Point(point.X - 1, point.Y));
                }
                if (point.Y > 0)
                {
                    floodStack.Add(new Point(point.X, point.Y - 1));
                }
                if (point.X < (mapSize - 1))
                {
                    floodStack.Add(new Point(point.X + 1, point.Y));
                }
                if (point.Y < (mapSize - 1))
                {
                    floodStack.Add(new Point(point.X, point.Y + 1));
                }
            }
        }
    }

    bool IsOneRoom()
    {
        Array.Clear(flood, 0, flood.Length);

        for (int i = 0; i < filled.Length; i++)
        {
            if (filled[i])
            {
                FloodFill(i % mapSize, i / mapSize);
                break;
            }
        }

        // Now if there are any parts of the room that aren't filled, it isn't one room.
        for (int i = 0; i < filled.Length; i++)
        {
            if (filled[i] && !flood[i])
            {
                return false;
            }
        }



        return true;
    }
}










/// tile map stuff

