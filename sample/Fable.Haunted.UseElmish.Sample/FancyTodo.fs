namespace Fable.Haunted.UseElmish.Sample

module Demo =

    open Elmish
    open Lit
    open Haunted
    open Haunted.Elmish
    open Browser.Types
    open System
    open 

    type Model = {
        Todos: string list
        AddNewValue: string
    }


    type Msg =
        | AddTodo of string
        | RemoveTodo of string
        | ChangeAddNewValue of string
        | LoadTodos
        | TodosLoaded of string list


    module Commands =

        let loadTodosCmd =
            fun dispatch ->
                async {
                    let! statusCode,content = Http.get "todos.json"
                    if statusCode = 200 then
                        let todos: string array = Fable.Core.JS.JSON.parse content :?> string array
                        dispatch <| TodosLoaded (todos |> Array.toList)
                }
                |> Async.StartImmediate
            |> Cmd.ofSub




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
        | LoadTodos ->
            state, Commands.loadTodosCmd
        | TodosLoaded todos ->
            { state with Todos = todos }, Cmd.none


    let viewLit state dispatch =
        let items = state.Todos |> List.map (fun i -> html $"""<li>{i}<button @click={fun _ -> dispatch <| RemoveTodo i}>Remove</button></li>""")
        
        let addTodo = fun _ -> dispatch <| AddTodo state.AddNewValue

        let keyup = 
            fun (ev:KeyboardEvent) -> 
                if (ev.key = "Enter") then
                    addTodo ()
                else
                    dispatch <| ChangeAddNewValue ev.target.Value 

        html $"""
            <h2>F#ncy Todo with Haunted and Lit</h2>
            {items}
            <input 
                .value={state.AddNewValue} 
                @keyup={keyup}>
            <button @click={addTodo}>Add Todo</button>
        """



    let fancy_todo_lit_element (props: {| todos: string option |}) =
        let todoStr = defaultArg props.todos ""
        let todos = todoStr.Split([|","|], StringSplitOptions.RemoveEmptyEntries) |> Array.toList;

        let state,dispatch = Haunted.useElmish(init, update, todos)

        viewLit state dispatch



    defineComponent "fancy-todo-lit" 
        (Haunted.Component(fancy_todo_lit_element, 
            {| 
                observedAttributes  = [| "todos" |]
                useShadowDom = true
            |}))
        

    
