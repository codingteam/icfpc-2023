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

    member private this.GetIndex(musicianId: int) (attendeeId: int): int =
        musicianId * this.MusiciansCount + attendeeId

    member this.SetDistance(musicianId: int) (attendeeId: int) (distance: double): MusicianAttendeeDistance =
        let index = this.GetIndex musicianId attendeeId
        { this with Distances = this.Distances.SetItem(index, distance) }

    member this.Distance(musicianId: int) (attendeeId: int): double =
        let index = this.GetIndex musicianId attendeeId
        this.Distances[index]

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

    member private this.GetIndex(musicianId: int) (attendeeId: int): int =
        musicianId * this.MusiciansCount + attendeeId

    member this.SetBlocks(musicianId: int) (attendeeId: int) (blocks: bool): MusicianBlocks =
        let index = this.GetIndex musicianId attendeeId
        { this with Blocks = this.Blocks.SetItem(index, blocks) }

    member this.IsSoundBlockedBetween(musicianId: int) (attendeeId: int): bool =
        let index = this.GetIndex musicianId attendeeId
        this.Blocks[index]

type PillarsBlocks =
    {
        MusiciansCount: int
        AttendeesCount: int
        PillarsCount: int
        Blocks: ImmutableArray<bool>
    }

    static member Create(musiciansCount: int) (attendeesCount: int) (pillarsCount: int): PillarsBlocks =
        let elements_count = musiciansCount * attendeesCount * pillarsCount
        let blocks = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            PillarsCount = pillarsCount
            Blocks = blocks
        }

    member private this.GetIndex(pillarId: int) (musicianId: int) (attendeeId: int): int =
        musicianId * this.MusiciansCount * this.AttendeesCount
            + attendeeId * this.AttendeesCount
            + pillarId

    member this.SetBlocks(pillarId: int) (musicianId: int) (attendeeId: int) (blocks: bool): PillarsBlocks =
        if this.PillarsCount > 0
        then
            let index = this.GetIndex pillarId musicianId attendeeId
            { this with Blocks = this.Blocks.SetItem(index, blocks) }
        else this

    member this.IsSoundBlockedBy(pillarId: int) (musicianId: int) (attendeeId: int): bool =
        if this.PillarsCount > 0
        then
            let index = this.GetIndex pillarId musicianId attendeeId
            this.Blocks[index]
        else false

type MusicianAttendeeImpact =
    {
        MusiciansCount: int
        AttendeesCount: int
        Impacts: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianAttendeeImpact =
        let elements_count = musiciansCount * attendeesCount
        let impacts = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Impacts = impacts
        }

    member private this.GetIndex(musicianId: int) (attendeeId: int): int =
        musicianId * this.MusiciansCount + attendeeId

    member this.SetImpact(musicianId: int) (attendeeId: int) (impact: double): MusicianAttendeeImpact =
        let index = this.GetIndex musicianId attendeeId
        { this with Impacts = this.Impacts.SetItem(index, impact) }

    member this.Impact(musicianId: int) (attendeeId: int): double =
        let index = this.GetIndex musicianId attendeeId
        this.Impacts[index]

type MusicianDistanceWithSameInstrument =
    {
        MusiciansCount: int
        Distances: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int): MusicianDistanceWithSameInstrument =
        let elements_count = musiciansCount * musiciansCount
        let distances = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            Distances = distances
        }

    member private this.GetIndex(fromId: int) (toId: int): int =
        if fromId > toId
        then fromId * this.MusiciansCount + toId
        else toId * this.MusiciansCount + fromId

    member this.SetDistance(fromMusicianId: int) (toMusicianId: int) (distance: double): MusicianDistanceWithSameInstrument =
        let index = this.GetIndex fromMusicianId toMusicianId
        { this with Distances = this.Distances.SetItem(index, distance) }

    member this.Distance(fromMusicianId: int) (toMusicianId: int): double =
        let index = this.GetIndex fromMusicianId toMusicianId
        this.Distances[index]

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

    member this.SetClosenessFactor(musicianId: int) (factor: double): MusicianClosenessFactor =
        { this with Factor = this.Factor.SetItem(musicianId, factor) }

    member this.ClosenessFactor(musicianId: int): double =
        this.Factor[musicianId]

type MusicianAttendeeTotalImpact =
    {
        MusiciansCount: int
        AttendeesCount: int
        Impacts: ImmutableArray<double>
    }

    static member zeroCreate(musiciansCount: int) (attendeesCount: int): MusicianAttendeeTotalImpact =
        let elements_count = musiciansCount * attendeesCount
        let impacts = (Array.zeroCreate elements_count).ToImmutableArray()
        {
            MusiciansCount = musiciansCount
            AttendeesCount = attendeesCount
            Impacts = impacts
        }

    member private this.GetIndex(musicianId: int) (attendeeId: int): int =
        musicianId * this.MusiciansCount + attendeeId

    member this.SetDistance(musicianId: int) (attendeeId: int) (impact: double): MusicianAttendeeTotalImpact =
        let index = this.GetIndex musicianId attendeeId
        { this with Impacts = this.Impacts.SetItem(index, impact) }

    member this.TotalImpact(musicianId: int) (attendeeId: int): double =
        let index = this.GetIndex musicianId attendeeId
        this.Impacts[index]

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

    /// Create initial state with given musician placements.
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
                MusicianDistanceWithSameInstrument = MusicianDistanceWithSameInstrument.zeroCreate problem.Musicians.Length
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

    /// Score for this solution.
    member this.Score: Score =
        if not this.IsValid
        then 0.0
        else failwith "unimplemented"
