using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Roles;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnPointSystem : EntitySystem
{
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning);
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        var possiblePositions = new List<EntityCoordinates>();

        if (TryComp<StationSpawningComponent>(args.Station, out var stationSpawning))
        {
            _stationSpawning.PopulateStationSpawnpoints(stationSpawning, args);
            var jobSpawnsDict = stationSpawning.JobSpawnPoints;
            var lateJoinSpawnsList = stationSpawning.LateJoinSpawnPoints;

            if (_gameTicker.RunLevel == GameRunLevel.InRound)
            {
                possiblePositions.AddRange(lateJoinSpawnsList);
            }
            else if (args.Job?.Prototype != null
                     && jobSpawnsDict.TryGetValue((ProtoId<JobPrototype>) args.Job.Prototype, out var coordinatesList))
            {
                possiblePositions.AddRange(coordinatesList);
            }
        }

        if (possiblePositions.Count == 0)
        {
            // Ok we've still not returned, but we need to put them /somewhere/.
            // TODO: Refactor gameticker spawning code so we don't have to do this!
            var points2 = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();

            if (points2.MoveNext(out _, out _, out var xform))
            {
                possiblePositions.Add(xform.Coordinates);
            }
            else
            {
                Log.Error("No spawn points were available!");
                return;
            }
        }

        var spawnLoc = _random.Pick(possiblePositions);

        // if (stationSpawning != null)
        // {
        //     // Remove spawnpoint from pool unless it's the last one for this job.
        //     if (args.Job?.Prototype != null
        //         && jobSpawnsDict.TryGetValue((ProtoId<JobPrototype>) args.Job.Prototype, out var currentJobSpawns)
        //         && currentJobSpawns.Count > 1)
        //     {
        //         currentJobSpawns.Remove(spawnLoc);
        //     }
        //
        //     stationSpawning.JobSpawnPoints = jobSpawnsDict;
        //     stationSpawning.LateJoinSpawnPoints = lateJoinSpawnsList;
        // }

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);
    }
}
