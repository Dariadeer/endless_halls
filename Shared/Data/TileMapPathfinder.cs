using Shared.MyMath;

namespace Shared.Data;

public class TileMapPathfinder
{
    static int2[] NeighbourOffsets = [int2.Up, int2.Down, int2.Left, int2.Right, new int2(1, 1), new int2(-1, -1)];
    private TileMap _tileMap;
    public TileMapPathfinder(TileMap tileMap)
    {
        _tileMap = tileMap;
    }

    public LinkedList<int2> BFS(int2 From, int2 To)
    {
        HashSet<Tile> visited = [];
        Queue<TileGraphNode> scheduled = [];

        scheduled.Enqueue(new TileGraphNode()
        {
            Tile = _tileMap[From]
        });

        while (scheduled.Count != 0)
        {
            var next = scheduled.Dequeue();
            if (next.Tile.Pos == To)
            {
                return PostProcess(next);
            }
            var neighbors = NeighbourOffsets.Select(offset => _tileMap.GetOrNull(next.Tile.Pos + offset));
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    if (neighbor.IsWalkable() && !visited.Contains(neighbor))
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

    public LinkedList<int2> AStar(int2 From, int2 To)
    {
        Dictionary<Tile, int> best = [];
        HashSet<Tile> visited = [];
        PriorityQueue<TileGraphNode, int> scheduled = new();

        scheduled.Enqueue(new TileGraphNode()
        {
            Tile = _tileMap[From],
            GraphDistance = 0,
            HeuristicDistance = CalculateH(From, To)
        }, CalculateH(From, To));

        while (scheduled.Count != 0)
        {

            var next = scheduled.Dequeue();
            visited.Add(next.Tile);
            if (next.Tile.Pos == To)
            {
                return PostProcess(next);
            }
            var neighbors = NeighbourOffsets.Select(offset => _tileMap.GetOrNull(next.Tile.Pos + offset));
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    if (neighbor.IsWalkable() && !visited.Contains(neighbor))
                    {
                        int g = next.GraphDistance + 1;
                        if (best.TryGetValue(neighbor, out int _g) && g >= _g)
                        {
                            continue;
                        }
                        best[neighbor] = g;
                        int h = CalculateH(neighbor.Pos, To);
                        scheduled.Enqueue(new TileGraphNode()
                        {
                            Tile = neighbor,
                            Source = next,
                            GraphDistance = g,
                            HeuristicDistance = h
                        }, g + h);
                    }
                }
            }
        }

        return [];
    }

    public LinkedList<int2> PostProcess(TileGraphNode node)
    {
        var result = new LinkedList<int2>();

        while (node.Source != null)
        {
            result.AddFirst(node.Tile.Pos);
            node = node.Source;
        }

        return result;
    }

    public int CalculateH(int2 pos1, int2 pos2)
    {
        var diff = pos1 - pos2;
        return Math.Max(Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y)), Math.Abs(diff.X - diff.Y));
    }
}

public class TileGraphNode
{
    public required Tile Tile;
    public TileGraphNode? Source = null;
    public int GraphDistance;
    public int HeuristicDistance;
}

public class PathNotFoundException : Exception
{
    public PathNotFoundException() : base("Could not find the path. The tile is either sealed off or unwalkable") { }
}
