using System.Collections.Generic;
using UnityEngine;


struct ThreeDTree
{
    public Dictionary<char, string> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public ThreeDTree(string init = "FA")
    {
        axiom = "FA";
        generations = 10;
        sizeCoefficient = 1.3f;
        rotationCoefficient = 10f;
        rules = new Dictionary<char, string>
            {
                {'A' , "^FB>B>>>B"},
                {'B' , "[^^-F>>>A]"}
            };
    }
}

struct ThreeDBinaryTree
{
    public Dictionary<char, string> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public ThreeDBinaryTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string>
            {
                {'F' , "FF"},
                {'X' , "F-[^[>X]<X]&[>X]<X"}
            };
    }
}

struct FractalPlant
{
    public Dictionary<char, string> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public FractalPlant(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.5f;
        rotationCoefficient = 20f;
        rules = new Dictionary<char, string>
            {
                {'F' , "FF"},
                {'X' , "F-^[<[X]&X]&F[&FX][>[X]&X]&F[&FX]^X"}
            };
    }
}

struct TwoDBinaryTree
{
    public Dictionary<char, string> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public TwoDBinaryTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string>
            {
                {'F' , "FF"},
                {'X' , "F-[^X]&X"}
            };
    }
}

// Spiral Tree, for testing rotataion and scaling coefficients
struct CurvyTree
{
    public Dictionary<char, string> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public CurvyTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string>
            {
                {'X' , "X-^X"}
            };
    }
}

struct BasicTree
{
    public Dictionary<char, string []> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public BasicTree(string init = "FA")
    {
        axiom = "A";
        generations = 8;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 25;
        rules = new Dictionary<char, string[]>
            {   //"X[-<A]", "X->A"
                {'A' , new[] {"XL-[-<A]>A", "XL-[<A]->A", "XL-[^A]-_A", "XL-[-^A]_A"}},
                {'L' , new[] {""}}
            };
    }
}

struct LeafyTree
{
    public Dictionary<char, string []> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public LeafyTree(string init = "FA")
    {
        axiom = "A";
        generations = 8;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 25;
        rules = new Dictionary<char, string[]>
            {   //"X[-<A]", "X->A"
                {'A' , new[] {"X-[-<B]>A", "X-[<A]->B", "X-[^B]-_A", "X-[-^A]_B"}},
                {'B' , new[] {"XL-[-<B]>B", "XL-[<B]->B", "XL-[^B]-_B", "XL-[-^B]_B"}},
                {'L' , new[] {"l"}},
                {'l' , new[] {""}}
            };
    }
}

struct PineTree
{
    public Dictionary<char, string []> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public PineTree(string init = "FA")
    {
        axiom = "A";
        generations = 7;
        sizeCoefficient = 1.3f;
        rotationCoefficient = 40;
        rules = new Dictionary<char, string[]>
            {   
                //"X-[-[<A]>A]A", "X-[-[>A]<A]A",
                {'A' , new[] {"X-[-[_A][<A][>A]^A]A" , "X-[-[<A][>A]]A", "X-[-[_A]^A]A", "X-[-_A]A", "X-[-^A]A"}},
            };
    }
}

// Binary 2D Tree
// public static Dictionary<char, string> rules = new Dictionary<char, string>
//     {
//         {'F' , "FF"},
//         {'X' , "F-[^X]&X"}
//     };

//     public static int generations = 4;
//     public static float sizeCoefficient = 1.2f;
//     public static float rotationCoefficient = 45f;