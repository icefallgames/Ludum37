using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour {

    public static int LevelNumber = 0;

    public AudioSource GasRelease;

    public GameObject WallPrefab;
    public GameObject SquarePrefab;
    public GameObject PlayerPrefab;
    public GameObject DoorPrefab;
    public GameObject BombPrefab;
    public GameObject PostPrefab;
    public GameObject SoundPrefab;
    public GameObject GasPrefab;
    public GameObject KeyPrefab;

    public GameObject EndingText;

    public GameObject SquaresParent;
    public GameObject WallsParent;

    private GameObject player;
    private SoundManager soundManager;

    public float InvalidFlashPeriod = 0.3f;

    public SoundManager GetSoundManager() {  return soundManager; }

    public void ReloadLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private DoorScript theDoor;

    void Awake()
    {
        soundManager = GameObject.Instantiate(SoundPrefab).GetComponent<SoundManager>();
    }

    // Use this for initialization
    void Start ()
    {
        if (LevelNumber < LevelData.Levels.Length)
        {
            things = LevelData.Levels[LevelNumber];
        }
        else
        {
            // End game (elsewhere we'll show a thank you message)
            things = new int[] { };
        }

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
            go.transform.parent = SquaresParent.transform;
            go.SetActive(false);
            allSquares.Add(go);
        }
    }

    private float invalidTimer;
    private bool invalidsOn;

    void ResetInvalidTimer()
    {
        invalidsOn = true;
        invalidTimer = InvalidFlashPeriod;
    }

    bool madeOnePush = false;

    void Update()
    {
        soundManager.NeedToExplain(!madeOnePush && (LevelNumber <= 1));
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

                        madeOnePush = true;

                        soundManager.PlayGrunt();
                        StartCoroutine(MaybePlayGruntInASec());
                        // Success. Time to regen!!!
                        CreateWalls();
                    }
                }

            }
        }

        invalidTimer -= Time.deltaTime;
        if (invalidTimer <= 0f)
        {
            Color highlight = new Color(0.5f, 0, 0);
            invalidTimer += InvalidFlashPeriod;
            invalidsOn = !invalidsOn;

            for (int i = 0; i < allSquares.Count; i++)
            {
                if (allSquares[i].activeSelf)
                {
                    allSquares[i].GetComponent<SpriteRenderer>().color = invalidsOn ? highlight : Color.black;
                }
                else
                {
                    break;
                }
            }
        }

        if (bombPresent && !startedDeath)
        {
            player.GetComponent<PlayerController>().DisablePlayer();
            startedDeath = true;
            StartCoroutine(DeathByPoison());
        }
    }




    private void MaybeAddPoison(List<Point> sources, int x, int y)
    {
        if ((x >= 0) && (y >= 0) && (y < mapSize) && (x < mapSize))
        {
            // If we haven't spread there and it's part of the room, then...
            if (!poisonSpread[x + y * mapSize] && filled[x + y * mapSize])
            {
                poisonSpread[x + y * mapSize] = true;
                sources.Add(new Point(x, y));
            }
        }
    }

    private bool[] poisonSpread = new bool[mapSize * mapSize];
    IEnumerator DeathByPoison()
    {
        GasRelease.Play();

        List<Point> poisonSources = new List<Point>();
        poisonSources.Add(bombPoint);
        poisonSpread[bombPoint.X + bombPoint.Y * mapSize] = true;
        while (poisonSources.Count > 0)
        {
            List<Point> newSources = new List<Point>();
            foreach (Point source in poisonSources)
            {
                GameObject gas = GameObject.Instantiate(GasPrefab);
                int x = source.X;
                int y = source.Y;
                gas.transform.position = new Vector3((float)x + 0.5f, (float)y + 0.5f, 0f);
                MaybeAddPoison(newSources, x - 1, y);
                MaybeAddPoison(newSources, x + 1, y);
                MaybeAddPoison(newSources, x, y - 1);
                MaybeAddPoison(newSources, x, y + 1);
            }
            poisonSources.Clear();
            poisonSources.AddRange(newSources);

            yield return new WaitForSeconds(0.4f);
        }

        yield return new WaitForSeconds(3.5f);
        ReloadLevel();
    }

    IEnumerator MaybePlayGruntInASec()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.8f, 1.2f));
        // 1 out of 3 times play yay
        if (UnityEngine.Random.Range(0, 3) == 0)
        {
            soundManager.PlaySuccess();
        }
    }

    public bool FailedToPushForReason()
    {
        // If there were invalids, then yeah:
        for (int i = 0; i < lastInvalid.Length; i++)
        {
            if (lastInvalid[i])
            {
                return true;
            }
        }

        // Or if we were blocked by a post?
        if (lastPushBlockedByPost)
        {
            return true;
        }

        return false;
    }

    void ClearInvalid()
    {
        Array.Clear(lastInvalid, 0, lastInvalid.Length);
        for (int i = 0; i < allSquares.Count; i++)
        {
            allSquares[i].SetActive(false);
        }
    }

    void SetInvalid(int xKnownGood, int yKnownGood)
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

            ResetInvalidTimer();
        }

    }

    private bool lastPushBlockedByPost = false;

    void GetMoveDelta(int direction, out int moveX, out int moveY)
    {
        moveX = 0;
        moveY = 0;
        switch (direction)
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
    }

    bool TryToPush(WallScript sourceWall)
    {
        int xSelected = sourceWall.X;
        int ySelected = sourceWall.Y;
        int moveX = 0;
        int moveY = 0;
        GetMoveDelta(sourceWall.Direction, out moveX, out moveY);

        bool success = false;

        bool blockedByPost = false;
        // Has to be empty in front of us
        if (IsEdge(xSelected, ySelected, moveX, moveY, out blockedByPost))
        {
            bool dummy;

            // Then we need to find how wide the wall is, and try to move it all.
            // Check left and right for edges
            int edgeDX = moveY;
            int edgeDY = -moveX;
            int min = 0;
            int xEdge = xSelected - edgeDX;
            int yEdge = ySelected - edgeDY;
            while (IsEdge(xEdge, yEdge, moveX, moveY, out dummy))
            {
                min--;
                xEdge -= edgeDX;
                yEdge -= edgeDY;
            }
            int max = 0;
            xEdge = xSelected + edgeDX;
            yEdge = ySelected + edgeDY;
            while (IsEdge(xEdge, yEdge, moveX, moveY, out dummy))
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
            if (!IsOneRoom(xSelected + moveX, ySelected + moveY))
            {
                // Restore if not
                // But first, store the invalid
                SetInvalid(xSelected + moveX, ySelected + moveY);
                Array.Copy(backup, filled, filled.Length);
                return false;
            }

            // Now push the player ahead a bit
            //player.transform.position += new Vector3(moveX, moveY, 0) * (1f - MinOffsetToPush) * 0.3f;

            // And slow him down - this does not seem to work.???
            // setting velocity directly no work too
            Rigidbody2D playerRigid = player.GetComponent<Rigidbody2D>();
            Vector3 newPlayerPos = player.transform.position;
            if (moveX != 0)
            {
                newPlayerPos.x = sourceWall.X + moveX + 0.5f;
            }
            if (moveY != 0)
            {
                newPlayerPos.y = sourceWall.Y + moveY + 0.5f;
            }
            // REVIEW: I think I'm supposed to use MovePosition to be nice with physics, but
            // if I don't move the player right away, he's still in the old position next frame, and causes
            // the walls to go wonky.
            player.transform.position = newPlayerPos;
            playerRigid.MovePosition(newPlayerPos);
            // f = m*a
            // Try adding an opposing force to slow him down
            //Debug.Log("Player v is " + playerRigid.velocity);
            //playerRigid.AddForce(-playerRigid.velocity * playerRigid.mass, ForceMode2D.Impulse);
            // Nope...
            // Try disabling input for a while
            player.GetComponent<PlayerController>().DisableBriefly();
            player.GetComponent<PlayerController>().HackForceCollisionExit(); // so he goes back to walking.

            // Freedom on both axes now.
            playerRigid.constraints = RigidbodyConstraints2D.FreezeRotation;

            lastPushBlockedByPost = false;

            success = true;
        }
        else
        {
            if (blockedByPost)
            {
                lastPushBlockedByPost = true;
            }
        }
        ClearInvalid();
        return success;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {

    }

    void LateUpdate()
    {
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
        GameObject key = null;
        for (int i = 0; i < things.Length; i++)
        {
            Vector3 pos = new Vector3((float)(i % mapSize) + 0.5f, (float)(i / mapSize) + 0.5f);
            switch (things[i])
            {
                case 1:
                    filled[i] = true;
                    // Put player in the first box we find
                    if (player == null)
                    {
                        player = GameObject.Instantiate(PlayerPrefab);
                        player.transform.position = pos;
                    }

                    break;
                case LevelData.D:
                    {
                        GameObject door = GameObject.Instantiate(DoorPrefab);
                        door.transform.position = pos;
                        theDoor = door.GetComponent<DoorScript>();
                    }
                    break;
                case LevelData.B:
                    {
                        GameObject bomb = GameObject.Instantiate(BombPrefab);
                        bomb.transform.position = pos;
                    }
                    break;
                case LevelData.P:
                    {
                        GameObject post = GameObject.Instantiate(PostPrefab);
                        post.transform.position = pos;
                    }
                    break;
                case LevelData.K:
                    {
                        key = GameObject.Instantiate(KeyPrefab);
                        key.transform.position = pos;
                    }
                    break;

            }
        }

        if (key != null)
        {
            // Door is closed!
            theDoor.SetIsOpen(false);
        }

        if (LevelNumber < LevelData.Levels.Length)
        {
            soundManager.PlayStart();
        }
        else
        {
            // Say bye
            EndingText.SetActive(true);
        }
    }

    private int[] things;


    private bool[] filled = new bool[mapSize * mapSize];
    private bool[] backup = new bool[mapSize * mapSize];
    private bool[] lastInvalid = new bool[mapSize * mapSize];
    private List<GameObject> allSquares = new List<GameObject>();


    private const int mapSize = 8;

    private Dictionary<WallScript, List<WallScript>> wallLinks = new Dictionary<WallScript, List<WallScript>>();
    private WallScript mainWallLink = null;

    bool GetAt(bool[] theArray, int x, int y)
    {
        return theArray[x + y * mapSize];
    }


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
    int GetThingBounded(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mapSize || y >= mapSize)
        {
            return 0;
        }
        return things[x + y * mapSize];
    }

    private List<WallScript> allWalls = new List<WallScript>();

    private WallScript FindWall(int x, int y, int direction, bool ignorePosts)
    {
        foreach (WallScript testWall in allWalls)
        {
            if ((testWall.Direction == direction) &&
                testWall.X == x &&
                testWall.Y == y)
            {
                // As long as there isn't a post in this direction!
                int moveX = 0;
                int moveY = 0;
                GetMoveDelta(direction, out moveX, out moveY);
                if (!ignorePosts && (LevelData.P == GetThingBounded(x + moveX, y + moveY)))
                {
                    return null;
                }
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
                    WallScript testWall = FindWall(xOrig, y, opposingDirection, ignorePosts: true);
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
                    WallScript testWall = FindWall(x, yOrig, opposingDirection, ignorePosts: true);
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
                    WallScript testWall = FindWall(xOrig, y, opposingDirection, ignorePosts: true);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;
            case 1:
                for (int x = xOrig; x >= 0; x--)
                {
                    WallScript testWall = FindWall(x, yOrig, opposingDirection, ignorePosts: true);
                    if (testWall != null)
                    {
                        return testWall;
                    }
                }
                break;
        }
        return null;
    }

    public float TimeBeforePush = 0.5f;
    public float MinOffsetToPush = 0.3f;

    private float pushTimer = 0f;

    public void OnStartPushWall(WallScript wall, Vector2 normal)
    {
        // The normal is basically the player's motion.
        // This won't prevent us from sticking, but... we could avoid hitting sidewalls accidentally.
        // It also means we can't get back inside!!! Unless I do opposite walls too.. yeah let's do that.
        // Hmm, this doesn't work well, since things are moving. Maybe I can restrict movement until we interact?
        // e.g. all walls are constrained in all things?
        /*
        int moveX, moveY;
        GetMoveDelta(wall.Direction, out moveX, out moveY);
        if (Mathf.Abs(Vector2.Dot(normal, new Vector2(moveX, moveY))) < 0.5f)
        {
            // Bail early... we're not hitting head on.
            return;
        }
        */


        // Do this before bailing... we might collide with neighbouring side walls while pushing another
        wall.WallBody.GetComponent<SpringJoint2D>().enabled = true;

        // Allow it freedom to move
        if ((wall.Direction % 2) == 0)
        {
            wall.WallBody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        }
        else
        {
            wall.WallBody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        }



        if (wallLinks.Count > 0)
        {
            return; // We're already pushing a wall
        }

        pushTimer = 0f;
        // TODO!
        // Ignore this completely if there is still another wall we're tracking!

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
                WallScript testWall = FindWall(x, yOrig, direction, ignorePosts: false);
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
                WallScript testWall = FindWall(x, yOrig, direction, ignorePosts: false);
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
                WallScript testWall = FindWall(xOrig, y, direction, ignorePosts: false);
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
                WallScript testWall = FindWall(xOrig, y, direction, ignorePosts: false);
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
            dependentWall.WallBody.GetComponent<Rigidbody2D>().isKinematic = true;
            dependentWall.WallBody.GetComponent<BoxCollider2D>().isTrigger = true;
        }

        wallLinks[wall] = dependentWalls;
        mainWallLink = wall;

        foreach (WallScript aWall in allWalls)
        {
            if ((aWall != wall) && !dependentWalls.Contains(aWall))
            {
                aWall.WallBody.GetComponent<InnerWall>().Highlight(false);
            }
            else
            {
                aWall.WallBody.GetComponent<InnerWall>().Highlight(true);
            }
        }

        // Fix the player's axis to avoid them sneaking out the side?
        // Hmm, this makes us stick... disabling it....
//        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation | (((direction % 2) == 0) ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.FreezePositionY);
    }

    public void OnStopPushWall(WallScript wall)
    {
        // Freedom on both axes now.
        player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;

        if (wallLinks.ContainsKey(wall))
        {
            // Uncolor these
            wall.WallBody.GetComponent<InnerWall>().Highlight(false);
            foreach (WallScript aWall in wallLinks[wall])
            {
                aWall.WallBody.GetComponent<InnerWall>().Highlight(false);
                // Restore it's position.
                aWall.WallBody.transform.position = aWall.WallBody.GetComponent<InnerWall>().OriginalPosition;
                aWall.WallBody.GetComponent<Rigidbody2D>().isKinematic = false; // Back to interacting with physics.
                aWall.WallBody.GetComponent<BoxCollider2D>().isTrigger = false; // Back to interacting with physics.
                aWall.WallBody.constraints = RigidbodyConstraints2D.FreezeAll; // Back to freeze all
            }

            // REVIEW: Now we only have one tracking ever.
            wallLinks.Clear();

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
        wall.transform.parent = WallsParent.transform;
        wall.transform.position = new Vector3((float)x + 0.5f, (float)y + 0.5f);
        wall.transform.rotation = Quaternion.AngleAxis(direction * 90f, new Vector3(0, 0, 1));
        Rigidbody2D wallBody = wall.GetComponent<WallScript>().WallBody;
        // For now, walls are constrained in all axes
        wall.GetComponent<WallScript>().SetLevel(this, x, y, direction);
        allWalls.Add(wall.GetComponent<WallScript>());
    }

    private bool doorIsHere = false;
    private bool keyIsHere = false;
    private bool bombPresent = false;
    private Point bombPoint;

    private bool startedDeath = false;

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

        doorIsHere = false;
        keyIsHere = false;

        // For now we'll create ones for each intersection
        // (might want to join them later)
        for (int y = 0; y < mapSize; y++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (GetAtBounded(x, y))
                {
                    if (things[x + y *mapSize] == LevelData.D)
                    {
                        // The door is in-bounds, we can win!
                        doorIsHere = true;
                    }
                    if (things[x + y * mapSize] == LevelData.K)
                    {
                        // The key is in bounds!
                        keyIsHere = true;
                    }
                    if (things[x + y * mapSize] == LevelData.B)
                    {
                        // Bomb!
                        bombPresent = true;
                        bombPoint = new Point(x, y);
                        soundManager.PlayDie();
                    }

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

        if (keyIsHere && doorIsHere)
        {
            theDoor.SetIsOpen(true);
        }
    }

    // tile map stuff

    private void SlideRow(int xStart, int yStart, int moveX, int moveY)
    {
        // Slide them over. 
        if (xStart >= 0 && yStart >= 0 && xStart < mapSize && yStart < mapSize)
        {
            // TODO: bounds checks
            int xMin = xStart;
            int yMin = yStart;
            int min = 0;
            while (GetAtBounded(xMin , yMin))
            {
                xMin -= moveX;
                yMin -= moveY;
                min--;
            }
            int xMax = xStart;
            int yMax = yStart;
            int max = 0;
            while (GetAtBounded(xMax, yMax))
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
    bool IsEdge(int x, int y, int moveX, int moveY, out bool blockedByPost)
    {
        blockedByPost = false;
        int newX = x + moveX;
        int newY = y + moveY;
        if (newX < 0 || newY < 0 || newX >= mapSize || newY >= mapSize)
        {
            return false;
        }

        if (things[newX + newY * mapSize] == LevelData.P)
        {
            blockedByPost = true;
            return false; // It's a post!
        }

        return GetAt(x, y) && !GetAt(newX, newY);
    }

    bool[] flood = new bool[mapSize * mapSize];

    List<Point> floodStack = new List<Point>();
    void FloodFill(bool[] theArray, int x, int y)
    {
        floodStack.Clear();
        floodStack.Add(new Point(x, y));
        while (floodStack.Count > 0)
        {
            Point point = floodStack[floodStack.Count - 1];
            floodStack.RemoveAt(floodStack.Count - 1);
            if (!GetAtFlood(point.X, point.Y) && GetAt(theArray, point.X, point.Y)) // not yet filled, but is part of room
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

    bool IsOneRoom(int xStart, int yStart)
    {
        Array.Clear(flood, 0, flood.Length);
        // Fill everything from where the player is (since they'll be in the new room)
        FloodFill(filled, xStart, yStart);

        bool isOneRoom = true;

        // Now if there are any parts of the room that aren't filled, it isn't one room.
        for (int i = 0; i < filled.Length; i++)
        {
            if (filled[i] && !flood[i])
            {
                isOneRoom = false;
                break;
            }
        }
        
        if (!isOneRoom)
        {
            // Remove the filled ones that were flooded (those are the ones that are in
            // the space space as the player)
            // In this case, filled[] is only used for display purposes, not to generate the new map
            for (int i = 0; i < filled.Length; i++)
            {
                if (filled[i] && flood[i])
                {
                    filled[i] = false;
                }
            }
        }

        return isOneRoom;
    }

    public void OnFoundDoor()
    {
        if (doorIsHere && !bombPresent && !isWinning) // e.g. door is in bounds.
        {
            // The wining conditions
            player.GetComponent<PlayerController>().DisablePlayer();
            isWinning = true;
            StartCoroutine(WinTheLevel());
        }
    }

    private bool isWinning = false;

    private IEnumerator WinTheLevel()
    {
        soundManager.PlayEnd();
        yield return new WaitForSeconds(2.5f);

        soundManager.PlayWinMusic();
        yield return new WaitForSeconds(2.3f);

        LevelNumber++;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}










/// tile map stuff

