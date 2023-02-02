using System;
using System.Linq;

namespace dotnet_sample_action.Helpers;

public static class RandomString
{
    
    private static readonly Random Random = new Random();
    
    public static string Generate(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz";
        return new string(Enumerable.Repeat(chars, length).Select(s => s[Random.Next(s.Length)]).ToArray());
    }
}
