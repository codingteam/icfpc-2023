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

type PillarsBlocks =
    {
        MusiciansCount: int
        AttendeesCount: int
        PillarsCount: int
        Blocks: ImmutableArray<bool>
    }

    static member Create(musiciansCount: int) (attendeesCount: int) (pillarsCount: int): PillarsBlocks =
        // FIXME: note that pillarsCount can be zero -- in that case, the answer is always false
        let elements_count = musiciansCount * attendeesCount * pillarsCount
        let blocks = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            PillarsCount = pillarsCount
            Blocks = blocks
        }

    // TODO: implement getter/setter with indexing
    // note that pillarsCount can be zero -- in that case, the answer is always false

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

type MusicianDistanceWithSameInstrument =
    {
        MusiciansCount: int
        AttendeesCount: int
        Distance: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianDistanceWithSameInstrument =
        let elements_count = musiciansCount * attendeesCount
        let distance = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Distance = distance
        }

    // TODO: implement getter/setter with indexing

type MusicianClosenessFactor =
    {
        MusiciansCount: int
        Factor: ImmutableArray<double>
    }

    static member Create(musiciansCount: int): MusicianClosenessFactor =
        let factors = (Array.create musiciansCount 1.0).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            Factor = factors
        }

    // TODO: implement getter/setter with indexing

type MusicianAttendeeTotalImpact =
    {
        MusiciansCount: int
        AttendeesCount: int
        Impact: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianAttendeeTotalImpact =
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
        MusicianPlacements: ImmutableArray<PointD>
        MusicianAttendeeDistance: MusicianAttendeeDistance
        MusicianBlocksOtherForAttendee: MusicianBlocks
        PillarBlocksSoundBetweenMusicianAndAttendee: PillarsBlocks
        MusicianAttendeeImpact: MusicianAttendeeImpact
        MusicianDistanceWithSameInstrument: MusicianDistanceWithSameInstrument
        MusicianClosenessFactor: MusicianClosenessFactor
        MusicianAttendeeTotalImpact: MusicianAttendeeTotalImpact
    }

    member private this.UpdateMusicianPlacement(musicianId: int, place: PointD): State =
        let new_musician_placements = this.MusicianPlacements.SetItem(musicianId, place)
        { this with MusicianPlacements = new_musician_placements }

    member private this.UpdateMusicianAttendeeDistances(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdateMusicianBlocks(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdatePillarsBlocks(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdateMusicianAttendeeImpact(musicianId: int): State =
        let state =
            this
                .UpdateMusicianAttendeeDistances(musicianId)
                .UpdateMusicianBlocks(musicianId)
                .UpdatePillarsBlocks(musicianId)
        failwith "unimplemented"

    member private this.UpdateMusicianDistanceToSameInstrument(musicianId: int): State =
        failwith "unimplemented"

    member private this.UpdateMusicianClosenessFactor(musicianId: int): State =
        let state = this.UpdateMusicianDistanceToSameInstrument(musicianId)
        failwith "unimplemented"

    member private this.UpdateMusicianAttendeeTotalImpact(musicianId: int): State =
        let state =
            this
                .UpdateMusicianAttendeeImpact(musicianId)
                .UpdateMusicianClosenessFactor(musicianId)
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
                PillarBlocksSoundBetweenMusicianAndAttendee = PillarsBlocks.Create problem.Musicians.Length problem.Attendees.Length problem.Pillars.Length
                MusicianAttendeeImpact = MusicianAttendeeImpact.zeroCreate problem.Musicians.Length problem.Attendees.Length
                MusicianDistanceWithSameInstrument = MusicianDistanceWithSameInstrument.zeroCreate problem.Musicians.Length problem.Attendees.Length
                MusicianClosenessFactor = MusicianClosenessFactor.Create problem.Musicians.Length
                MusicianAttendeeTotalImpact = MusicianAttendeeTotalImpact.zeroCreate problem.Musicians.Length problem.Attendees.Length
            }
        Seq.indexed musician_placements
        |> Seq.fold (fun state (i, position) -> state.PlaceMusician(i, position)) initialState

    /// Put musician at a given place and return updated state. Leave the original state unmodified
    member this.PlaceMusician(musicianId: int, place: PointD): State =
        this
            .UpdateMusicianPlacement(musicianId, place)
            .UpdateMusicianAttendeeTotalImpact(musicianId)

    /// Checks if all musicians are far enough from stage edges and each other.
    member this.IsValid: bool =
        failwith "unimplemented"

    member this.Score: Score =
        if not this.IsValid
        then 0.0
        else failwith "unimplemented"
