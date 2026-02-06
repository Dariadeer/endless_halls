using Shared.Math;

namespace Shared.Data;

public class TileMapPathfinder
{
    static Int2[] NeighbourOffsets = [Int2.Up, Int2.Down, Int2.Left, Int2.Right, new Int2(1, 1), new Int2(-1, -1)];
    private TileMap _tileMap;
    public TileMapPathfinder(TileMap tileMap)
    {
        _tileMap = tileMap;
    }

    public LinkedList<Int2> BFS(Int2 From, Int2 To)
    {
        HashSet<Tile> visited = [];
        Queue<TileGraphNode> scheduled = [];

        scheduled.Enqueue(new TileGraphNode()
        {
            Tile = _tileMap[From]
        });

        while(scheduled.Count != 0)
        {
            var next = scheduled.Dequeue();
            if(next.Tile.Pos == To)
            {
                return PostProcess(next);
            }
            var neighbors = NeighbourOffsets.Select(offset => _tileMap.GetOrNull(next.Tile.Pos + offset));
            foreach (var neighbor in neighbors) {
                if(neighbor != null)
                {
                    if(neighbor.IsWalkable() && !visited.Contains(neighbor))
                    {
                        scheduled.Enqueue(new TileGraphNode()
                        {
                            Tile = neighbor,
                            Source = next
                        });
                    }
                }
            }
            visited.Add(next.Tile);
        }

        return [];
    }

    public LinkedList<Int2> PostProcess(TileGraphNode node)
    {
        var result = new LinkedList<Int2>();

        while(node.Source != null)
        {
            result.AddFirst(node.Tile.Pos);
            node = node.Source;
        }

        return result;
    }
}

public class TileGraphNode
{
    public Tile Tile;
    public TileGraphNode? Source = null;
} 

public class PathNotFoundException : Exception
{
    public PathNotFoundException() : base("Could not find the path. The tile is either sealed off or unwalkable") {}
}