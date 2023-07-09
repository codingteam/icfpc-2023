module Icfpc2023.IterativeScoring

open System.Collections.Immutable

type private MusicianPlacements = PointD[]

type MusicianAttendeeDistance =
    {
        MusiciansCount: int
        AttendeesCount: int
        Distances: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianAttendeeDistance =
        let elements_count = musiciansCount * attendeesCount
        let distances = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Distances = distances
        }

    // TODO: implement getter/setter with indexing

type MusicianBlocks =
    {
        MusiciansCount: int
        AttendeesCount: int
        Blocks: ImmutableArray<bool>
    }

    static member Create(musiciansCount: int) (attendeesCount: int): MusicianBlocks =
        let elements_count = musiciansCount * musiciansCount * attendeesCount
        let blocks = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Blocks = blocks
        }

    // TODO: implement getter/setter with indexing

type MusicianAttendeeImpact =
    {
        MusiciansCount: int
        AttendeesCount: int
        Impact: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianAttendeeImpact =
        let elements_count = musiciansCount * attendeesCount
        let impact = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Impact = impact
        }

    // TODO: implement getter/setter with indexing

type State =
    {
        Problem: Problem
        MusicianPlacements: ImmutableArray<PointD> // [0]
        MusicianAttendeeDistance: MusicianAttendeeDistance // [1]
        MusicianBlocksOtherForAttendee: MusicianBlocks // [6]
        MusicianAttendeeImpact: MusicianAttendeeImpact // [2]

        // TODO: add a matrix of distances from each musician to each other musician playing the same instrument -- depends on 0. [3]

        // TODO: add a vector of closeness factor (length == number of musicians) -- depends on 3. [4]

        // TODO: add matrix of closeness*impact (for each attendee-musician pair) -- depends on 4 and 2. [5]
    }

    member private this.UpdateMusicianPlacement(musicianId: int, place: PointD): State =
        let new_musician_placements = this.MusicianPlacements.SetItem(musicianId, place)
        { this with MusicianPlacements = new_musician_placements }

    member private this.UpdateMusicianAttendeeDistances(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdateMusicianBlocks(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdateMusicianAttendeeImpact(musicianId: int): State =
        let state =
            this
                .UpdateMusicianAttendeeDistances(musicianId)
                .UpdateMusicianBlocks(musicianId)
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
                MusicianAttendeeDistance = MusicianAttendeeDistance.zeroCreate problem.Musicians.Length problem.Attendees.Length
                MusicianBlocksOtherForAttendee = MusicianBlocks.Create problem.Musicians.Length problem.Attendees.Length
                MusicianAttendeeImpact = MusicianAttendeeImpact.zeroCreate problem.Musicians.Length problem.Attendees.Length
            }
        Seq.indexed musician_placements
        |> Seq.fold (fun state (i, position) -> state.PlaceMusician(i, position)) initialState

    /// Put musician at a given place and return updated state. Leave the original state unmodified
    member this.PlaceMusician(musicianId: int, place: PointD): State =
        this
            .UpdateMusicianPlacement(musicianId, place)
            .UpdateMusicianAttendeeImpact(musicianId)
        // TODO: signal to other fields that this musician moved so they can re-calculate stuff

    /// Checks if all musicians are far enough from stage edges and each other.
    member this.IsValid: bool =
        failwith "unimplemented"

    member this.Score: Score =
        if not this.IsValid
        then 0.0
        else failwith "unimplemented"
