namespace Fable.Haunted.UseElmish

open Haunted
open Elmish

[<AutoOpen>]
module UseElmishExtensions =
    type private ElmishObservable<'Model, 'Msg>() =
        let mutable state: 'Model option = None
        let mutable listener: ('Model -> unit) option = None
        let mutable dispatcher: ('Msg -> unit) option = None

        member _.Value = state

        member _.SetState (model: 'Model) (dispatch: 'Msg -> unit) =
            state <- Some model
            dispatcher <- Some dispatch
            match listener with
            | None -> ()
            | Some listener -> listener model

        member _.Dispatch(msg) =
            match dispatcher with
            | None -> () // Error?
            | Some dispatch -> dispatch msg

        member _.Subscribe(f) =
            match listener with
            | Some _ -> ()
            | None -> listener <- Some f

        

    let private runProgram (program: unit -> Program<'Arg, 'Model, 'Msg, unit>) (arg: 'Arg) (obs: ElmishObservable<'Model, 'Msg>) () =
        program()
        |> Program.withSetState obs.SetState
        |> Program.runWith arg

        match obs.Value with
        | None -> failwith "Elmish program has not initialized"
        | Some v -> v        

    let disposeState (state: obj) =
        match box state with
        | :? System.IDisposable as disp -> disp.Dispose()
        | _ -> ()
    

    type Haunted with

        static member useElmish(program: unit -> Program<'Arg, 'Model, 'Msg, unit>, arg: 'Arg, ?dependencies: obj array) =
            let obs, _ = Haunted.useState<ElmishObservable<'Model, 'Msg>> (fun () -> ElmishObservable<'Model, 'Msg>())
            let state, setState = Haunted.useState<'Model>(runProgram program arg obs)

            Haunted.useEffect((fun () ->
                runProgram program arg obs () |> setState
            ), defaultArg dependencies [||])

            obs.Subscribe(setState)
            state, obs.Dispatch

        static member useElmish(program: unit -> Program<unit, 'Model, 'Msg, unit>, ?dependencies: obj array) =
            Haunted.useElmish(program, (), ?dependencies=dependencies)

        static member useElmish(init: 'Arg -> 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, arg: 'Arg, ?dependencies: obj array) =
            Haunted.useElmish((fun () -> Program.mkProgram init update (fun _ _ -> ())), arg, ?dependencies=dependencies)

        static member useElmish(init: unit -> 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, ?dependencies: obj array) =
            Haunted.useElmish((fun () -> Program.mkProgram init update (fun _ _ -> ())), ?dependencies=dependencies)

        static member useElmish(init: 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, ?dependencies: obj array) =
            Haunted.useElmish((fun () -> Program.mkProgram (fun () -> init) update (fun _ _ -> ())), ?dependencies=dependencies)
