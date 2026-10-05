using Shared.Network;
using Shared.Network.Messages;
using Shared.MyMath;

namespace Shared.Data;

public class World : ISnapshot<World>, ISerializable<World>, IServerMessageable
{
    private static readonly HashSet<int2> EmptyTilePositions = [];
    private static readonly int2[] VisionDirections =
    [
        int2.Right,
        int2.Down,
        new int2(-1, -1),
        int2.Left,
        int2.Up,
        new int2(1, 1),
    ];

    public readonly TileMap Grid;
    public readonly EntityMap Entities;
    public readonly TileMapPathfinder Pathfinder;
    private readonly Dictionary<int, HashSet<int2>> _visibleTilesByTeam = [];
    private readonly Dictionary<int, HashSet<int2>> _currentVisionTilesByTeam = [];
    private readonly Dictionary<int, HashSet<int2>> _exploredTilesByTeam = [];
    public IReadOnlyDictionary<int, HashSet<int2>> VisibleTilesByTeam => _visibleTilesByTeam;
    public IReadOnlyDictionary<int, HashSet<int2>> CurrentVisionTilesByTeam => _currentVisionTilesByTeam;
    public IReadOnlyDictionary<int, HashSet<int2>> ExploredTilesByTeam => _exploredTilesByTeam;
    public int VisionRevision { get; private set; }
    public Action<Entity>? EntityAppeared;
    public Action<int>? EntityDisappeared;

    public static ServerMessageType MessageType => ServerMessageType.WorldState;

    public World(TileMap tileMap, EntityMap entityMap)
    {
        Grid = tileMap;
        Entities = entityMap;
        Pathfinder = new(tileMap);

        foreach (var entity in Entities.Values)
        {
            AttachEntity(entity);
        }

        foreach (var teamId in Entities.Values.Select(entity => entity.TeamId).Distinct())
        {
            RecalculateTeamVision(teamId);
        }
    }

    public void SummonEntity(Entity entity)
    {
        AttachEntity(entity);
        Entities.AddEntity(entity);
        RecalculateTeamVision(entity.TeamId);
        SeedCurrentVisionAt(entity);
        EntityAppeared?.Invoke(entity);
    }

    public void RemoveEntity(Entity entity)
    {
        int teamId = entity.TeamId;
        entity.VisionAreaChanged -= OnEntityVisionAreaChanged;
        entity.TeamChanged -= OnEntityTeamChanged;
        Entities.Remove(entity.Id);
        RecalculateTeamVision(teamId);
        entity.Disappear();
    }

    private void AttachEntity(Entity entity)
    {
        entity.AttachGrid(Grid);
        entity.VisionAreaChanged += OnEntityVisionAreaChanged;
        entity.TeamChanged += OnEntityTeamChanged;
    }

    private void OnEntityVisionAreaChanged(Entity entity)
    {
        RecalculateTeamVision(entity.TeamId);
    }

    private void OnEntityTeamChanged(Entity entity, int previousTeamId)
    {
        RecalculateTeamVision(previousTeamId);
        RecalculateTeamVision(entity.TeamId);
        SeedCurrentVisionAt(entity);
    }

    private void SeedCurrentVisionAt(Entity entity)
    {
        if (_currentVisionTilesByTeam.TryGetValue(entity.TeamId, out var currentVisionTiles)
            && _visibleTilesByTeam.TryGetValue(entity.TeamId, out var totalVisionTiles)
            && totalVisionTiles.Contains(entity.VisionOrigin)
            && currentVisionTiles.Add(entity.VisionOrigin))
        {
            if (_exploredTilesByTeam.TryGetValue(entity.TeamId, out var exploredTiles))
            {
                exploredTiles.Add(entity.VisionOrigin);
            }
            VisionRevision++;
        }
    }

    private void RecalculateTeamVision(int teamId)
    {
        if (!_visibleTilesByTeam.TryGetValue(teamId, out var visibleTiles))
        {
            visibleTiles = [];
            _visibleTilesByTeam[teamId] = visibleTiles;
        }
        else
        {
            visibleTiles.Clear();
        }

        bool initializeCurrentVision = !_currentVisionTilesByTeam.TryGetValue(teamId, out var currentVisionTiles);
        if (initializeCurrentVision)
        {
            currentVisionTiles = [];
            _currentVisionTilesByTeam[teamId] = currentVisionTiles;
        }

        if (!_exploredTilesByTeam.TryGetValue(teamId, out var exploredTiles))
        {
            exploredTiles = [];
            _exploredTilesByTeam[teamId] = exploredTiles;
        }

        foreach (var entity in Entities.Values)
        {
            if (entity.TeamId != teamId)
            {
                continue;
            }

            foreach (var tile in entity.VisionArea)
            {
                visibleTiles.Add(tile.Pos);
            }
        }

        if (initializeCurrentVision)
        {
            currentVisionTiles!.UnionWith(visibleTiles);
            exploredTiles!.UnionWith(currentVisionTiles);
        }

        VisionRevision++;
    }

    public bool IsTileVisibleToTeam(int teamId, int2 pos)
    {
        return _visibleTilesByTeam.TryGetValue(teamId, out var tiles) && tiles.Contains(pos);
    }

    public bool IsTileInCurrentVisionForTeam(int teamId, int2 pos)
    {
        return _currentVisionTilesByTeam.TryGetValue(teamId, out var tiles) && tiles.Contains(pos);
    }

    public bool IsTileExploredByTeam(int teamId, int2 pos)
    {
        return _exploredTilesByTeam.TryGetValue(teamId, out var tiles) && tiles.Contains(pos);
    }

    public IReadOnlySet<int2> GetExploredTilesForTeam(int teamId)
    {
        return _exploredTilesByTeam.TryGetValue(teamId, out var tiles)
            ? tiles
            : EmptyTilePositions;
    }

    public void AdvanceCurrentVision(int tick)
    {
        bool shouldShrink = (tick + 1) % 20 == 0;
        bool shouldExpand = (tick + 1) % 10 == 0;
        if (!shouldShrink && !shouldExpand)
        {
            return;
        }

        foreach (var (teamId, currentVisionTiles) in _currentVisionTilesByTeam)
        {
            bool changed = false;
            _visibleTilesByTeam.TryGetValue(teamId, out var totalVisionTiles);
            totalVisionTiles ??= [];

            // At overlapping intervals, peel stale outer tiles first, then
            // advance the current vision one layer into total vision.
            if (shouldShrink)
            {
                var toRemove = new List<int2>();
                foreach (var pos in currentVisionTiles)
                {
                    if (totalVisionTiles.Contains(pos))
                    {
                        continue;
                    }

                    int currentNeighborCount = 0;
                    foreach (var direction in VisionDirections)
                    {
                        if (currentVisionTiles.Contains(pos + direction))
                        {
                            currentNeighborCount++;
                        }
                    }

                    if (currentNeighborCount < VisionDirections.Length)
                    {
                        toRemove.Add(pos);
                    }
                }

                foreach (var pos in toRemove)
                {
                    currentVisionTiles.Remove(pos);
                }

                changed |= toRemove.Count > 0;
            }

            if (shouldExpand && totalVisionTiles.Count > 0)
            {
                var toAdd = new HashSet<int2>();
                foreach (var pos in currentVisionTiles)
                {
                    foreach (var direction in VisionDirections)
                    {
                        var neighborPos = pos + direction;
                        if (totalVisionTiles.Contains(neighborPos))
                        {
                            toAdd.Add(neighborPos);
                        }
                    }
                }

                foreach (var pos in toAdd)
                {
                    changed |= currentVisionTiles.Add(pos);
                }
            }

            if (changed)
            {
                if (_exploredTilesByTeam.TryGetValue(teamId, out var exploredTiles))
                {
                    exploredTiles.UnionWith(currentVisionTiles);
                }
                VisionRevision++;
            }
        }
    }

    public void Advance(int tick)
    {
        foreach (var entity in Entities.Values)
        {
            var movement = entity.Movement;
            if (movement.State == MovementState.Moving && movement.End == tick)
            {
                entity.CompleteMovement(tick);
            }
        }

        AdvanceCurrentVision(tick);
    }

    public World Copy()
    {
        var copy = new World(
            Grid.Copy(),
            Entities.Copy()
        );

        foreach (var (teamId, tiles) in _exploredTilesByTeam)
        {
            if (!copy._exploredTilesByTeam.TryGetValue(teamId, out var exploredTiles))
            {
                exploredTiles = [];
                copy._exploredTilesByTeam[teamId] = exploredTiles;
            }

            exploredTiles.UnionWith(tiles);
        }

        foreach (var (teamId, tiles) in _currentVisionTilesByTeam)
        {
            if (!copy._currentVisionTilesByTeam.TryGetValue(teamId, out var currentVisionTiles))
            {
                currentVisionTiles = [];
                copy._currentVisionTilesByTeam[teamId] = currentVisionTiles;
            }

            currentVisionTiles.Clear();
            currentVisionTiles.UnionWith(tiles);
        }

        return copy;
    }

    public void Encode(BinaryWriter writer)
    {
        Grid.Encode(writer);
        Entities.Encode(writer);

        writer.Write(_exploredTilesByTeam.Count);
        foreach (var (teamId, tiles) in _exploredTilesByTeam)
        {
            writer.Write(teamId);
            writer.Write(tiles.Count);
            foreach (var pos in tiles)
            {
                pos.Encode(writer);
            }
        }

        writer.Write(_currentVisionTilesByTeam.Count);
        foreach (var (teamId, tiles) in _currentVisionTilesByTeam)
        {
            writer.Write(teamId);
            writer.Write(tiles.Count);
            foreach (var pos in tiles)
            {
                pos.Encode(writer);
            }
        }
    }

    public static World Decode(BinaryReader reader)
    {
        var world = new World(TileMap.Decode(reader), EntityMap.Decode(reader));

        int teamCount = reader.ReadInt32();
        for (int i = 0; i < teamCount; i++)
        {
            int teamId = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            if (!world._exploredTilesByTeam.TryGetValue(teamId, out var exploredTiles))
            {
                exploredTiles = [];
                world._exploredTilesByTeam[teamId] = exploredTiles;
            }

            for (int j = 0; j < tileCount; j++)
            {
                exploredTiles.Add(int2.Decode(reader));
            }
        }

        int currentTeamCount = reader.ReadInt32();
        for (int i = 0; i < currentTeamCount; i++)
        {
            int teamId = reader.ReadInt32();
            int tileCount = reader.ReadInt32();
            if (!world._currentVisionTilesByTeam.TryGetValue(teamId, out var currentVisionTiles))
            {
                currentVisionTiles = [];
                world._currentVisionTilesByTeam[teamId] = currentVisionTiles;
            }
            else
            {
                currentVisionTiles.Clear();
            }

            for (int j = 0; j < tileCount; j++)
            {
                currentVisionTiles.Add(int2.Decode(reader));
            }
        }

        return world;
    }
}
