using System;
using System.Collections.Generic;
using UnityEngine;

namespace OctreeTest
{
    public class InvalidPathException : Exception
    {
        public InvalidPathException(string path)
            : base("Invalid path part " + path)
        {
        }
    }
}
