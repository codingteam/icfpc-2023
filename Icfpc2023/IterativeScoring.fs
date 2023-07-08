module Icfpc2023.IterativeScoring

open System.Collections.Immutable

type private MusicianPlacements = PointD[]

type State = {
    Problem: Problem
    MusicianPlacements: ImmutableArray<PointD> // [0]

    // TODO: add a matrix of attendee-musician distances (squared) -- depends on 0. [1]

    // TODO: add a 3D matrix of musician-musician-attendee bools indicating if the first musician blocks the second musician's sound for this attendee -- depends on 0. [6]

    // TODO: add a matrix of musician impact on each attendee -- depends on 1 and 6. [2]

    // TODO: add a matrix of distances from each musician to each other musician playing the same instrument -- depends on 0. [3]

    // TODO: add a vector of closeness factor (length == number of musicians) -- depends on 3. [4]

    // TODO: add matrix of closeness*impact (for each attendee-musician pair) -- depends on 4 and 2. [5]
    }

let InitState(problem: Problem, musician_placements: PointD[]): State =
    {
        Problem = problem
        MusicianPlacements = musician_placements.ToImmutableArray()
    }

/// Put musician at a given place and return updated state. Leave the original state unmodified
let PlaceMusician(state: State, musicianId: int, place: PointD): State =
    let new_musician_placements = state.MusicianPlacements.SetItem(musicianId, place)
    // TODO: signal to other fields that this musician moved so they can re-calculate stuff
    { state with
        MusicianPlacements = new_musician_placements
    }

let CalculateScore(state: State): Score = failwith "unimplemented"
