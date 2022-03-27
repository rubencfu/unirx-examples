using System;
using System.Collections.Generic;
using UnityEngine;

public class Tags : MonoBehaviour
{
    private static string[] holeTags = {"Stone", "Wood", "Metal", "Sand"};

    [SerializeField]
    private List<string> tags = new List<string>();


    public bool HasTag(string tag) => tags.Contains(tag);

    public string getHoleTag() => tags.Find(HoleTag);

    private static bool HoleTag(string t) => Array.Exists(holeTags, x => x == t);

    public IEnumerable<string> GetTags() => tags;
}
