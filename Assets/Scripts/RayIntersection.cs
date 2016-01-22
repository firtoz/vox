using System;
using System.Collections.Generic;
using UnityEngine;

public abstract partial class OctreeBase<TItem, TNode, TTree>
    where TTree : OctreeBase<TItem, TNode, TTree> where TNode : OctreeNodeBase<TItem, TTree, TNode> {
    public class RayIntersection {
        private readonly byte _a;
        private readonly bool _debug;
        private readonly bool _intersectMultiple;

        private readonly TNode _rootNode;
        private readonly Transform _transform;

        public readonly List<RayIntersectionResult> results = new List<RayIntersectionResult>();
        private Ray _ray;

        public RayIntersection(Transform transform, TTree octree, Ray r, bool intersectMultiple, int? wantedDepth = null,
            bool debug = false) {
            _debug = debug;
            _rootNode = octree.GetRoot();
            if (_rootNode == null) {
                //we had a good run guys, time to call it a day
                return;
            }

            _transform = transform;
            _ray = r;
            _intersectMultiple = intersectMultiple;
            _a = 0;

            var ro = transform.InverseTransformPoint(r.origin);
            var rd = transform.InverseTransformDirection(r.direction);

            var rootBounds = _rootNode.GetBounds();

            var rox = ro.x;
            var roy = ro.y;
            var roz = ro.z;

            var rdx = rd.x;
            var rdy = rd.y;
            var rdz = rd.z;

            var ocMin = rootBounds.min;
            var ocMax = rootBounds.max;

            var rootCenter = rootBounds.center;
            if (rdx < 0.0f) {
                rox = rootCenter.x - rox;
                rdx = -rdx;
                _a |= 1;
            }

            if (rdy < 0.0f) {
                roy = rootCenter.y - roy;
                rdy = -rdy;
                _a |= 2;
            }

            if (rdz < 0.0f) {
                roz = rootCenter.z - roz;
                rdz = -rdz;
                _a |= 4;
            }

            float tx0, tx1, ty0, ty1, tz0, tz1;

            if (!Mathf.Approximately(rdx, 0.0f)) {
                tx0 = (ocMin.x - rox) / rdx;
                tx1 = (ocMax.x - rox) / rdx;
            } else {
                tx0 = 99999.9f;
                tx1 = 99999.9f;
            }

            if (!Mathf.Approximately(rdy, 0.0f)) {
                ty0 = (ocMin.y - roy) / rdy;
                ty1 = (ocMax.y - roy) / rdy;
            } else {
                ty0 = 99999.9f;
                ty1 = 99999.9f;
            }

            if (!Mathf.Approximately(rdz, 0.0f)) {
                tz0 = (ocMin.z - roz) / rdz;
                tz1 = (ocMax.z - roz) / rdz;
            } else {
                tz0 = 99999.9f;
                tz1 = 99999.9f;
            }

            if (Mathf.Max(tx0, ty0, tz0) < Mathf.Min(tx1, ty1, tz1)) {
                ProcSubtree(tx0, ty0, tz0, tx1, ty1, tz1, _rootNode, _rootNode.IsSolid(), 0, wantedDepth,
                    _rootNode.GetCoords());
            }
        }

        private static EntryPlane GetEntryPlane(float tx0, float ty0, float tz0) {
            if (tx0 > ty0) {
                if (tx0 > tz0) {
                    //z greatest
                    return EntryPlane.YZ;
                }
            } else if (ty0 > tz0) {
                //y greatest
                return EntryPlane.XZ;
            }

            //x greatest

            return EntryPlane.XY;
        }


        private static int FirstNode(float tx0, float ty0, float tz0, float txm, float tym, float tzm) {
            var entryPlane = GetEntryPlane(tx0, ty0, tz0);

            var firstNode = 0;

            switch (entryPlane) {
                case EntryPlane.XY:
                    if (txm < tz0) {
                        firstNode |= 1;
                    }
                    if (tym < tz0) {
                        firstNode |= 2;
                    }
                    break;
                case EntryPlane.XZ:
                    if (txm < ty0) {
                        firstNode |= 1;
                    }
                    if (tzm < ty0) {
                        firstNode |= 4;
                    }
                    break;
                case EntryPlane.YZ:
                    if (tym < tx0) {
                        firstNode |= 2;
                    }
                    if (tzm < tx0) {
                        firstNode |= 4;
                    }
                    break;
            }

            return firstNode;
        }

        private static int NewNode(double x, int xi, double y, int yi, double z, int zi) {
            if (x < y) {
                if (x < z) {
                    return xi;
                }
            } else if (y < z) {
                return yi;
            }

            return zi;
        }

        private void DrawLocalLine(Vector3 a, Vector3 b, Color color, bool force = false) {
            if (_debug || force) {
                Debug.DrawLine(_transform.TransformPoint(a), _transform.TransformPoint(b), color, 0, false);
            }
        }

        private void ProcSubtree(float tx0, float ty0, float tz0, float tx1, float ty1, float tz1, TNode node,
            bool insideSolidNode, int currentDepth, int? wantedDepth,
            Coords nodeCoords) {
            if (!_intersectMultiple && results.Count > 0) {
                return;
            }

            if (wantedDepth == null) {
                if (node == null) {
                    return;
                }

                if (node.IsSolid()) {
                    ProcessTerminal(node, tx0, ty0, tz0);
                    return;
                }
            } else {
                if (!insideSolidNode) {
                    //didn't manage to get into a solid node
                    if (node == null) {
                        return;
                    }

                    insideSolidNode = node.IsSolid();
                }

                if (insideSolidNode && currentDepth >= wantedDepth) {
                    if (currentDepth == wantedDepth) {
                        ProcessTerminal(nodeCoords, tx0, ty0, tz0);
                    } else {
                        //oops, went too deep!!!
                        //trace back to wanted depth

                        var newCoords = new OctreeChildCoords[wantedDepth.Value];

                        for (var i = 0; i < newCoords.Length; ++i) {
                            newCoords[i] = nodeCoords.GetCoord(i);
                        }

                        ProcessTerminal(new Coords(newCoords), tx0, ty0,
                            tz0);
                    }
                    return;
                }
            }

            if (node != null) {
                var bounds = node.GetBounds();
                DrawBounds(bounds, Color.white);
            } else {
                //inside solid node and still going strong baby!
                var bounds = _rootNode.GetChildBounds(nodeCoords);
                DrawBounds(bounds, Color.cyan);
            }

            if (tx1 < 0.0 || ty1 < 0.0 || tz1 < 0.0) {
                return;
            }

            var txm = 0.5f * (tx0 + tx1);
            var tym = 0.5f * (ty0 + ty1);
            var tzm = 0.5f * (tz0 + tz1);

            var currNode = FirstNode(tx0, ty0, tz0, txm, tym, tzm);

            while (currNode < 8) {
                var childIndex = (OctreeNode.ChildIndex) (currNode ^ _a);
                if (!_intersectMultiple && results.Count > 0) {
                    return;
                }

                var nextDepth = currentDepth + 1;
                var childCoords = new Coords(nodeCoords,
                    OctreeChildCoords.FromIndex(childIndex));

                TNode childNode;

                if (insideSolidNode) {
                    childNode = null;
                } else {
                    childNode = node.GetChild(childIndex);
                }

                switch (currNode) {
                    //0= none
                    //1 = only x
                    //2 = only y
                    //3 = 2 + 1 = y and x
                    //4 = only z
                    //5 = 4 + 1 = z and x
                    //6 = 4 + 2 = z and y
                    //7 = 4 + 2 + 1 = z and y and x
                    //z sets 4, y set 2, x sets 1
                    //except if the bit is already set, then it can't set it again so 8
                    case 0:
                        //0= none
                        ProcSubtree(tx0, ty0, tz0, txm, tym, tzm, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(txm, 1, tym, 2, tzm, 4);
                        break;
                    case 1:
                        //1 = only x
                        ProcSubtree(txm, ty0, tz0, tx1, tym, tzm, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(tx1, 8, tym, 3, tzm, 5);
                        break;
                    case 2:
                        //2 = only y
                        ProcSubtree(tx0, tym, tz0, txm, ty1, tzm, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(txm, 3, ty1, 8, tzm, 6);
                        break;
                    case 3:
                        //3 = 2 + 1 = y and x
                        ProcSubtree(txm, tym, tz0, tx1, ty1, tzm, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(tx1, 8, ty1, 8, tzm, 7);
                        break;
                    case 4:
                        //4 = only z
                        ProcSubtree(tx0, ty0, tzm, txm, tym, tz1, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(txm, 5, tym, 6, tz1, 8);
                        break;
                    case 5:
                        //5 = 4 + 1 = z and x
                        ProcSubtree(txm, ty0, tzm, tx1, tym, tz1, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(tx1, 8, tym, 7, tz1, 8);
                        break;
                    case 6:
                        //6 = 4 + 2 = z and y
                        ProcSubtree(tx0, tym, tzm, txm, ty1, tz1, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = NewNode(txm, 7, ty1, 8, tz1, 8);
                        break;
                    case 7:
                        //7 = 4 + 2 + 1 = z and y and x
                        ProcSubtree(txm, tym, tzm, tx1, ty1, tz1, childNode, insideSolidNode, nextDepth, wantedDepth,
                            childCoords);
                        currNode = 8;
                        break;
                }
            }
        }

        private void DrawBounds(Bounds bounds, bool force = false) {
            DrawBounds(bounds, Color.white, force);
        }

        private void DrawBounds(Bounds bounds, Color color, bool force = false) {
            var min = bounds.min;
            var max = bounds.max;

            DrawLocalLine(min, new Vector3(min.x, min.y, max.z), color, force);
            DrawLocalLine(min, new Vector3(min.x, max.y, min.z), color, force);
            DrawLocalLine(min, new Vector3(max.x, min.y, min.z), color, force);

            DrawLocalLine(new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), color, force);

            DrawLocalLine(new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z), color, force);
            DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z), color, force);
            DrawLocalLine(new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z), color, force);

            DrawLocalLine(max, new Vector3(max.x, max.y, min.z), color, force);
            DrawLocalLine(max, new Vector3(max.x, min.y, max.z), color, force);
            DrawLocalLine(max, new Vector3(min.x, max.y, max.z), color, force);

            DrawLocalLine(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z), color, force);
            DrawLocalLine(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z), color, force);
        }

        private Vector3 GetNormal(EntryPlane entryPlane) {
            Vector3 normal;
            switch (entryPlane) {
                case EntryPlane.XY:
                    if ((_a & 4) == 0) {
                        normal = Vector3.back;
                    } else {
                        normal = Vector3.forward;
                    }

                    break;
                case EntryPlane.XZ:
                    if ((_a & 2) == 0) {
                        normal = Vector3.down;
                    } else {
                        normal = Vector3.up;
                    }
                    break;
                case EntryPlane.YZ:
                    if ((_a & 1) == 0) {
                        normal = Vector3.left;
                    } else {
                        normal = Vector3.right;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException("entryPlane", entryPlane, null);
            }

            return normal;
        }


        private NeighbourSide GetNeighbourSide(EntryPlane entryPlane) {
            switch (entryPlane) {
                case EntryPlane.XY:
                    if ((_a & 4) == 0) {
                        return NeighbourSide.Back;
                    }
                    return NeighbourSide.Forward;
                case EntryPlane.XZ:
                    if ((_a & 2) == 0) {
                        return NeighbourSide.Below;
                    }
                    return NeighbourSide.Above;
                case EntryPlane.YZ:
                    if ((_a & 1) == 0) {
                        return NeighbourSide.Left;
                    }
                    return NeighbourSide.Right;
                default:
                    throw new ArgumentOutOfRangeException("entryPlane", entryPlane, null);
            }
        }

        private void ProcessTerminal(TNode node, float tx0, float ty0, float tz0) {
            var entryDistance = Mathf.Max(tx0, ty0, tz0);

            var entryPlane = GetEntryPlane(tx0, ty0, tz0);

            var normal = GetNormal(entryPlane);

            var size = 1f;
            Debug.DrawLine(_ray.origin, _ray.GetPoint(entryDistance), Color.white, 0, false);

            Debug.DrawLine(_ray.GetPoint(entryDistance),
                _ray.GetPoint(entryDistance) + _transform.TransformDirection(normal) * size, Color.green, 0, false);

            var bounds = node.GetBounds();
            DrawBounds(bounds, Color.cyan, true);

            var childBounds =
                node.GetChildBounds(
                    new Coords(new[]
                    {OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftAboveBack)}));

            DrawBounds(childBounds, Color.green, true);

            childBounds = node.GetChildBounds(new Coords(
                new[] {OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightAboveBack)}));

            DrawBounds(childBounds, Color.green, true);

            childBounds = node.GetChildBounds(new Coords(
                new[] {OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.RightAboveForward)}));

            DrawBounds(childBounds, Color.green, true);

            childBounds = node.GetChildBounds(new Coords(
                new[] {OctreeChildCoords.FromIndex(OctreeNode.ChildIndex.LeftAboveForward)}));

            DrawBounds(childBounds, Color.green, true);

            results.Add(new RayIntersectionResult(node, node.GetCoords(), entryDistance, _ray.GetPoint(entryDistance),
                normal, GetNeighbourSide(entryPlane)));
        }

        private void ProcessTerminal(Coords nodeCoords, float tx0, float ty0,
            float tz0) {
            var entryDistance = Mathf.Max(tx0, ty0, tz0);

            var entryPlane = GetEntryPlane(tx0, ty0, tz0);

            var normal = GetNormal(entryPlane);

            var size = 1f;
            Debug.DrawLine(_ray.origin, _ray.GetPoint(entryDistance), Color.white, 0, false);

            Debug.DrawLine(_ray.GetPoint(entryDistance),
                _ray.GetPoint(entryDistance) + _transform.TransformDirection(normal) * size, Color.green, 0, false);


            var bounds = _rootNode.GetChildBounds(nodeCoords);
            DrawBounds(bounds, Color.red, true);

            results.Add(new RayIntersectionResult(null, nodeCoords, entryDistance, _ray.GetPoint(entryDistance),
                normal, GetNeighbourSide(entryPlane)));
        }

        private enum EntryPlane {
            XY,
            XZ,
            YZ
        }
    }
}