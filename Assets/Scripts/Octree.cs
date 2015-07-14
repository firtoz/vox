using System.Collections.Generic;
using UnityEngine;

public class Octree<T>
{
    private readonly OctreeNode<T> _root;

    public Octree(Bounds bounds)
    {
        _root = new OctreeNode<T>(bounds);
    }

    public OctreeNode<T> GetRoot()
    {
        return _root;
    } 

    // https://en.wikipedia.org/wiki/Breadth-first_search#Pseudocode

    /*
1     procedure BFS(G,v) is
2      let Q be a queue
3      Q.enqueue(v)
4      label v as discovered
5      while Q is not empty
6         v ← Q.dequeue()
7         process(v)
8         for all edges from v to w in G.adjacentEdges(v) do
9             if w is not labeled as discovered
10                 Q.enqueue(w)
11                label w as discovered
    */
    public IEnumerable<OctreeNode<T>> BreadthFirst()
    {
        return _root.BreadthFirst();
    }

    // https://en.wikipedia.org/wiki/Depth-first_search#Pseudocode
    /*
    1  procedure DFS-iterative(G,v):
    2      let S be a stack
    3      S.push(v)
    4      while S is not empty
    5            v = S.pop() 
    6            if v is not labeled as discovered:
    7                label v as discovered
    8                for all edges from v to w in G.adjacentEdges(v) do
    9                    S.push(w)
    */
    public IEnumerable<OctreeNode<T>> DepthFirst()
    {
        return _root.DepthFirst();
    }

    public void AddBounds(Bounds bounds, int i)
    {
        _root.AddBounds(bounds, i);
    }
}
