using System;
using Assets.Scripts;

public class VoxelCoordinates : OctreeNodeBase<int, VoxelTree, VoxelNode, VoxelCoordinates>.Coordinates {
    public VoxelCoordinates() {}
    public VoxelCoordinates(VoxelTree tree) : base(tree) {}

    public VoxelCoordinates(VoxelTree tree, VoxelCoordinates parentCoordinates,
        params OctreeChildCoordinates[] furtherChildren) : base(tree, parentCoordinates, furtherChildren) {}

    public VoxelCoordinates(VoxelTree tree, OctreeChildCoordinates[] coords) : base(tree, coords) {}

    public override VoxelCoordinates Construct(VoxelTree voxelTree) {
        return new VoxelCoordinates(voxelTree);
    }

    public override VoxelCoordinates Construct(VoxelTree voxelTree, OctreeChildCoordinates[] newCoords) {
        return new VoxelCoordinates(voxelTree, newCoords);
    }

    public override VoxelCoordinates Construct(VoxelTree voxelTree, VoxelCoordinates nodeCoordinates,
        OctreeChildCoordinates octreeChildCoordinates) {
        return new VoxelCoordinates(voxelTree, nodeCoordinates, octreeChildCoordinates);
    }


    public VoxelCoordinates GetNeighbourCoords(NeighbourSide side) {
        OctreeChildCoordinates[] newCoords;

        var voxelTree = tree;

        if (coords.Length > 0) {
            newCoords = new OctreeChildCoordinates[coords.Length];

            var hasLastCoords = false;
            var lastCoordX = 0;
            var lastCoordY = 0;
            var lastCoordZ = 0;

            for (var i = coords.Length - 1; i >= 0; --i) {
                var coord = coords[i];

                var currentX = coord.x;
                var currentY = coord.y;
                var currentZ = coord.z;

                if (hasLastCoords) {
                    //let's check the lower coords, if it's out of that bounds then we need to modify ourselves!
                    var lastCoordUpdated = UpdateLastCoord(
                        ref lastCoordX, ref currentX,
                        ref lastCoordY, ref currentY,
                        ref lastCoordZ, ref currentZ);

                    if (lastCoordUpdated) {
                        newCoords[i + 1] = new OctreeChildCoordinates(lastCoordX, lastCoordY, lastCoordZ);
                    }
                } else {
                    //final coords!
                    //update coords from the side
                    switch (side) {
                        case NeighbourSide.Above:
                            currentY += 1;
                            break;
                        case NeighbourSide.Below:
                            currentY -= 1;
                            break;
                        case NeighbourSide.Right:
                            currentX += 1;
                            break;
                        case NeighbourSide.Left:
                            currentX -= 1;
                            break;
                        case NeighbourSide.Back:
                            currentZ -= 1;
                            break;
                        case NeighbourSide.Forward:
                            currentZ += 1;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("side", side, null);
                    }
                }

                var newCoord = new OctreeChildCoordinates(currentX, currentY, currentZ);
                newCoords[i] = newCoord;

                lastCoordX = currentX;
                lastCoordY = currentY;
                lastCoordZ = currentZ;
                hasLastCoords = true;
            }

            // we're at the end now

            if (hasLastCoords) {
                if (lastCoordX < 0 || lastCoordX > 1 ||
                    lastCoordY < 0 || lastCoordY > 1 ||
                    lastCoordZ < 0 || lastCoordZ > 1) {
                    //invalid coords, out of bounds, pick neighbour voxelTree

                    var currentX = lastCoordX;
                    var currentY = lastCoordY;
                    var currentZ = lastCoordZ;

                    UpdateLastCoord(ref lastCoordX, ref currentX,
                        ref lastCoordY, ref currentY,
                        ref lastCoordZ, ref currentZ);

                    newCoords[0] = new OctreeChildCoordinates(lastCoordX, lastCoordY, lastCoordZ);
                    if (tree == null) {
                        voxelTree = null;
                    } else {
                        voxelTree = tree.GetOrCreateNeighbour(side);
                    }
                }
            }
        } else {
            newCoords = null;
        }

        return newCoords == null ? null : new VoxelCoordinates(voxelTree, newCoords);
    }


    private static bool UpdateLastCoord(ref int lastCoordX, ref int currentX, ref int lastCoordY, ref int currentY,
        ref int lastCoordZ, ref int currentZ) {
        var updateLastCoord = false;

        if (lastCoordX < 0) {
            currentX -= 1;
            lastCoordX = 1;
            updateLastCoord = true;
        } else if (lastCoordX > 1) {
            currentX += 1;
            lastCoordX = 0;
            updateLastCoord = true;
        }

        if (lastCoordY < 0) {
            currentY -= 1;
            lastCoordY = 1;
            updateLastCoord = true;
        } else if (lastCoordY > 1) {
            currentY += 1;
            lastCoordY = 0;
            updateLastCoord = true;
        }

        if (lastCoordZ < 0) {
            currentZ -= 1;
            lastCoordZ = 1;
            updateLastCoord = true;
        } else if (lastCoordZ > 1) {
            currentZ += 1;
            lastCoordZ = 0;
            updateLastCoord = true;
        }
        return updateLastCoord;
    }
}