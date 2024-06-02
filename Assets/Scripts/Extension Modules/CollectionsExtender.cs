using System;
using System.Collections.Generic;
using System.Linq;

//https://stackoverflow.com/questions/10443461/c-sharp-array-findallindexof-which-findall-indexof
//thank you random Nikhil Agrawal
public static class CollectionsExtender
{
    public static int[] FindAllIndexof<T>(this IEnumerable<T> values, T val)
    {
        return values.Select((b, i) => object.Equals(b, val) ? i : -1).Where(i => i != -1).ToArray();
    }
}