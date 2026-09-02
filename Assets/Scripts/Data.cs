using System.Collections.Generic;
using UnityEngine;

public class Data
{
    
    


}

struct ThreeDTree {
        public Dictionary<char, string> rules;

        public string axiom;

        public int generations;
        public float sizeCoefficient;
        public float rotationCoefficient;

        public ThreeDTree(string init = "FA")
        {
            axiom = init;
            generations = 10;
            sizeCoefficient = 1.3f;
            rotationCoefficient = 10f;
            rules = new Dictionary<char, string>
            {
                {'A' , "^FB>>B>>>>>B"},
                {'B' , "[^^-F>>>>>>A]"}
            };
        }


}

struct ThreeDBinaryTree {
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
                {'X' , "F-[<[^X]&X]>[^X]&X"}
            };
        }
}

struct FractalPlant {
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

struct TwoDBinaryTree {
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

// Binary 2D Tree
// public static Dictionary<char, string> rules = new Dictionary<char, string>
//     {
//         {'F' , "FF"},
//         {'X' , "F-[^X]&X"}
//     };

//     public static int generations = 4;
//     public static float sizeCoefficient = 1.2f;
//     public static float rotationCoefficient = 45f;