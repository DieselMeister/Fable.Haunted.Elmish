namespace Fable.Haunted.UseElmish.Sample

module Demo =

    open Elmish
    open Lit
    open Haunted
    open Haunted.Elmish
    open Browser.Types
    open System

    type Model = {
        Todos: string list
        AddNewValue: string
    }


    type Msg =
        | AddTodo of string
        | RemoveTodo of string
        | ChangeAddNewValue of string


    let init initTodos =
        { Todos = initTodos; AddNewValue = "" }, Cmd.none


    let update msg state =
        match msg with
        | AddTodo todo ->
            { state with Todos = todo::state.Todos; AddNewValue = "" }, Cmd.none
        | RemoveTodo todo ->
            { state with Todos = state.Todos |> List.filter (fun i -> i<> todo)}, Cmd.none
        | ChangeAddNewValue value ->
            { state with AddNewValue = value }, Cmd.none


    let viewLit state dispatch =
        let items = state.Todos |> List.map (fun i -> html $"""<li>{i}</li><button @click={fun _ -> dispatch <| RemoveTodo i}>Remove</button>""")
        html $"""
            <h2>F#ncy Todo with Haunted and Lit</h2>
            {items}
            <input .value={state.AddNewValue} @keyup={fun (ev:Event) -> dispatch <| ChangeAddNewValue ev.target.Value })>
            <button @click={fun _ -> dispatch <| AddTodo state.AddNewValue}>Add Todo</button>
        """



    let fancy_todo_lit_element (props: {| todos: string option |}) =
        let todoStr = defaultArg props.todos ""
        let todos = todoStr.Split([|","|], StringSplitOptions.RemoveEmptyEntries) |> Array.toList;

        let state,dispatch = Haunted.useElmish(init, update, todos)

        viewLit state dispatch



    defineComponent "fancy_todo_lit" 
        (Haunted.Component(fancy_todo_lit_element, 
            {| 
                observableAttributes = [| "todos"|]
                useShadowDom = true
            |}))
        

    
