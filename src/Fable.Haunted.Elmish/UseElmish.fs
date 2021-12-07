namespace Haunted.Elmish

open Haunted
open Elmish
open Lit.Types
open Lit


module InjectStyle =

    open Fable.Core
    open Fable.Core.JsInterop
    emitJsStatement () """import { hook, Hook } from "haunted" """
    
    let useStyles : ((unit->CSSResult)->unit)  =
        emitJsExpr () """
            hook (
                class extends Hook {
                    constructor(id, state, fn, values) {
                        super(id, state);
                        this.values = values || [];
                        this.ignoreChanges = this.values.length === 0;
                        this.updateStyles(fn);
                    }
    
                    update(fn, values) {
                        if (this.hasChanged(values)) {
                            console.log("Changed");
                            this.values = values;
                            this.updateStyles(fn);
                        }
                    }
    
                    updateStyles(fn) {
                        let styles = fn();
                        if (this.state.host && this.state.host.shadowRoot) {
                            this.state.host.shadowRoot.adoptedStyleSheets = [styles.styleSheet];
                        } else if (this.state.host) {
                            this.state.node.adoptedStyleSheets = [styles.styleSheet];
                        }
                        else {
                            console.warn("Could not append Stylesheet to Shadow DOM. Your browser might not support this, try using ShadyCSS polyfill.");
                        }
                    }
    
                    hasChanged(values) {
                        if (this.ignoreChanges) return false;
                        return values.some((value, i) => this.values[i] !== value);
                    }
                }
            );
        """





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

        static member useElmish(program: unit -> Program<'Arg, 'Model, 'Msg, unit>, arg: 'Arg, ?styling:Lit.CSSResult list, ?dependencies: obj array) =
            let obs, _ = Haunted.useState<ElmishObservable<'Model, 'Msg>> (fun () -> ElmishObservable<'Model, 'Msg>())
            let state, setState = Haunted.useState<'Model>(runProgram program arg obs)

            styling
            |> Option.iter (fun stylings ->
                stylings
                |> List.iter (fun styling ->
                    InjectStyle.useStyles (fun () -> styling)
                )
            )

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

        
        
        static member useElmishWithStyling(program: unit -> Program<unit, 'Model, 'Msg, unit>, ?styling:Lit.CSSResult list, ?dependencies: obj array) =
            Haunted.useElmish(program, (), ?styling = styling, ?dependencies=dependencies)

        static member useElmishWithStyling(init: 'Arg -> 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, arg: 'Arg, ?styling:Lit.CSSResult list, ?dependencies: obj array) =
            Haunted.useElmish((fun () -> Program.mkProgram init update (fun _ _ -> ())), arg, ?styling = styling, ?dependencies=dependencies)

        static member useElmishWithStyling(init: unit -> 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, ?styling:Lit.CSSResult list, ?dependencies: obj array) =
            Haunted.useElmishWithStyling((fun () -> Program.mkProgram init update (fun _ _ -> ())), ?styling = styling, ?dependencies=dependencies)

        static member useElmishWithStyling(init: 'Model * Cmd<'Msg>, update: 'Msg -> 'Model -> 'Model * Cmd<'Msg>, ?styling:Lit.CSSResult list, ?dependencies: obj array) =
            Haunted.useElmishWithStyling((fun () -> Program.mkProgram (fun () -> init) update (fun _ _ -> ())), ?styling = styling, ?dependencies=dependencies)
