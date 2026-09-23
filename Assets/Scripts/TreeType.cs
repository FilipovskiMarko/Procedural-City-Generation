using UnityEngine;
using System.Collections.Generic;

public class TreeType : MonoBehaviour
{
    [System.NonSerialized] public Dictionary<char, string []> rules;
    public string axiom;
    public int generations;
    public float sizeCoefficient;
    public float rotationCoefficient;

    public TreeType(string init = "FA")
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

class BasicTree : TreeType
{
    public BasicTree(string init = "FA")
    {
        axiom = "A";
        generations = 9;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 25;
        rules = new Dictionary<char, string[]>
            {   //"X[-<A]", "X->A"
                {'A' , new[] {"XL-[-<A]>A", "XL-[<A]->A", "XL-[^A]-_A", "XL-[-^A]_A"}},
                {'L' , new[] {""}}
            };
    }
}

class LeafyTree : TreeType 
{
    public LeafyTree(string init = "FA")
    {
        axiom = "A";
        generations = 9;
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

class ThreeDTree : TreeType
{
    public ThreeDTree(string init = "FA")
    {
        axiom = "FA";
        generations = 10;
        sizeCoefficient = 1.3f;
        rotationCoefficient = 10f;
        rules = new Dictionary<char, string[]>
            {
                {'A' , new[] {"^FB>B>>>B"}},
                {'B' , new[] {"[^^-F>>>A]"}}
            };
    }
}

class ThreeDBinaryTree : TreeType
{
    public ThreeDBinaryTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string[]>
            {
                {'F' , new[] {"FF"}},
                {'X' , new[] {"F-[^[>X]<X]&[>X]<X"}}
            };
    }
}

class FractalPlant : TreeType
{
    public FractalPlant(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.5f;
        rotationCoefficient = 20f;
        rules = new Dictionary<char, string[]>
            {
                {'F' , new[] {"FF"}},
                {'X' , new[] {"F-^[<[X]&X]&F[&FX][>[X]&X]&F[&FX]^X"}}
            };
    }
}

class TwoDBinaryTree : TreeType
{
    public TwoDBinaryTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string[]>
            {
                {'F' , new[] {"FF"}},
                {'X' , new[] {"F-[^X]&X"}}
            };
    }
}

// Spiral Tree, for testing rotataion and scaling coefficients
class CurvyTree : TreeType
{
    public CurvyTree(string init = "FA")
    {
        axiom = "X";
        generations = 4;
        sizeCoefficient = 1.2f;
        rotationCoefficient = 45f;
        rules = new Dictionary<char, string[]>
            {
                {'X' , new[] {"X-^X"}}
            };
    }
}





class PineTree : TreeType
{
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

