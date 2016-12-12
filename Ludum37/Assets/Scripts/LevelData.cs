using UnityEngine;
using System.Collections;

public static class LevelData
{
    public const int D = 2;    // door
    public const int B = 3;    // bomb
    public const int P = 4;    // post
    public const int K = 5;    // key

    // Just shows how to get to door
    public static int[] Level0 = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,D,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,1,1,0,0,
            0,0,0,1,1,1,0,0,
            0,0,0,0,1,1,0,0,
            0,0,0,0,0,0,0,0,
    };

    // Poison in the way
    public static int[] Level1 = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,1,0,0,0,0,
            0,1,1,1,1,0,0,D,
            0,0,0,0,1,0,B,0,
            0,0,0,0,0,0,0,0,
    };

    // Poison a bit harder to circumvent
    public static int[] Level2 = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,1,1,D,0,0,
            0,0,1,1,B,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
    };

    // This was hard for me, but I eventually did it... yikes!
    public static int[] Level3 = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,P,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,1,1,1,1,
            0,B,D,B,0,1,1,0,
            0,0,0,0,0,0,0,0,
    };

    // Introduce the key
    public static int[] Level4 = new int[]
    {
            0,0,0,0,0,0,K,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,D,0,
            0,0,0,0,0,0,1,0,
            0,0,0,0,0,0,1,0,
            0,0,0,0,0,0,1,0,
            0,0,0,0,0,0,1,0,
    };


    // Wow, it totally is doable! Key and door far apart, you need to form a long line.
    public static int[] Level5 = new int[]
    {
            0,0,0,0,0,0,0,0,
            0,1,1,0,1,0,0,0,
            0,1,1,1,1,0,0,0,
            0,1,0,0,0,0,0,0,
            0,0,0,0,B,0,0,0,
            D,0,0,P,P,0,0,0,
            0,0,0,0,0,0,K,0,
            0,0,0,0,0,0,0,0,
    };

    // Let's try... door surrounded by bombs... takes a while, but ok.
    public static int[] Level6 = new int[]
    {
            0,0,1,0,1,0,1,0,
            0,0,1,1,1,1,1,1,
            K,0,P,0,0,0,0,P,
            0,0,0,0,0,0,0,0,
            0,0,0,B,D,B,0,0,
            0,0,0,B,B,B,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
    };

    // Holy crap! It IS possible! wrap around, then join!
    public static int[] Level7 = new int[]
    {
            0,0,0,0,0,1,1,1,
            0,B,0,0,D,1,1,1,
            P,0,0,P,0,1,1,1,
            0,0,0,K,0,B,0,0,
            0,0,0,0,0,B,0,0,
            P,0,0,P,0,0,0,0,
            0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,
    };


    public static int[][] Levels = new int[][]
    {
        Level0,
        Level1,
        Level2,
        Level3,
        Level4,
        Level5,
        Level6,
        Level7,
    };
}
