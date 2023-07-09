module Icfpc2023.IterativeScoring

open System.Collections.Immutable

type private MusicianPlacements = PointD[]

type State =
    {
        Problem: Problem
        MusicianPlacements: ImmutableArray<PointD> // [0]
        MusicianAttendeeDistance: ImmutableArray<ImmutableArray<double>> // [1]

        // TODO: add a 3D matrix of musician-musician-attendee bools indicating if the first musician blocks the second musician's sound for this attendee -- depends on 0. [6]

        // TODO: add a matrix of musician impact on each attendee -- depends on 1 and 6. [2]

        // TODO: add a matrix of distances from each musician to each other musician playing the same instrument -- depends on 0. [3]

        // TODO: add a vector of closeness factor (length == number of musicians) -- depends on 3. [4]

        // TODO: add matrix of closeness*impact (for each attendee-musician pair) -- depends on 4 and 2. [5]
    }

    member private this.UpdateMusicianAttendeeDistances(musicianId: int): State =
        failwith "unimplemented"

    static member Create(problem: Problem, musician_placements: PointD[]): State =
        let musician_attendee_distance =
            let builder = ImmutableArray.CreateBuilder<ImmutableArray<double>>()
            for i in 0 .. musician_placements.Length-1 do
                builder.Add((Array.zeroCreate musician_placements.Length).ToImmutableArray())
            builder.ToImmutable()
        let initialState =
            {
                Problem = problem
                MusicianPlacements = musician_placements.ToImmutableArray()
                MusicianAttendeeDistance = musician_attendee_distance
            }
        Seq.indexed musician_placements
        |> Seq.fold (fun state (i, position) -> state.PlaceMusician(i, position)) initialState

    /// Put musician at a given place and return updated state. Leave the original state unmodified
    member this.PlaceMusician(musicianId: int, place: PointD): State =
        let new_musician_placements = this.MusicianPlacements.SetItem(musicianId, place)
        let state =
            { this with
                MusicianPlacements = new_musician_placements
            }
        state.UpdateMusicianAttendeeDistances(musicianId)
        // TODO: signal to other fields that this musician moved so they can re-calculate stuff

    /// Checks if all musicians are far enough from stage edges and each other.
    member this.IsValid: bool =
        failwith "unimplemented"

    member this.Score: Score =
        if not this.IsValid
        then 0.0
        else failwith "unimplemented"
