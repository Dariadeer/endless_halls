using Shared.MyMath;

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

    public LinkedList<Int2> AStar(Int2 From, Int2 To)
    {
        Dictionary<Tile, int> best = [];
        HashSet<Tile> visited = [];
        PriorityQueue<AStarTileGraphNode, int> scheduled = new();

        scheduled.Enqueue(new AStarTileGraphNode()
        {
            Tile = _tileMap[From],
            G = 0,
            H = CalculateH(From, To)
        }, CalculateH(From, To));

        while(scheduled.Count != 0)
        {
            
            var next = scheduled.Dequeue();
            visited.Add(next.Tile);
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
                        int g = next.G + 1;
                        if(best.TryGetValue(neighbor, out int _g) && g >= _g)
                        {
                            continue;
                        }
                        best[neighbor] = g;
                        int h = CalculateH(neighbor.Pos, To);
                        scheduled.Enqueue(new AStarTileGraphNode()
                        {
                            Tile = neighbor,
                            Source = next,
                            G = g,
                            H = h
                        }, g + h);
                    }
                }
            }
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

    public int CalculateH(Int2 pos1, Int2 pos2)
    {
        var diff = pos1 - pos2;
        return Math.Max(Math.Max(Math.Abs(diff.X), Math.Abs(diff.Y)), Math.Abs(diff.X - diff.Y));
    }
}

public class TileGraphNode
{
    public Tile Tile;
    public TileGraphNode? Source = null;
} 

public class AStarTileGraphNode : TileGraphNode
{
    public int G;
    public int H;
} 

public class PathNotFoundException : Exception
{
    public PathNotFoundException() : base("Could not find the path. The tile is either sealed off or unwalkable") {}
}