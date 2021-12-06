namespace Fable.Haunted.UseElmish.Sample

module Demo =

    open Elmish
    open Lit
    open Haunted
    open Haunted.Elmish
    open Browser.Types
    open System
    open Fable.SimpleHttp

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
        let items = 
            state.Todos 
            |> List.map (fun i -> html $"""<li><span>{i}</span><button @click={fun _ -> dispatch <| RemoveTodo i}>Remove</button></li>""")
        
        let addTodo = fun _ -> dispatch <| AddTodo state.AddNewValue

        let keyup = 
            fun (ev:KeyboardEvent) -> 
                if (ev.key = "Enter") then
                    addTodo ()
                else
                    dispatch <| ChangeAddNewValue ev.target.Value 

        html $"""
            <h2>F#ncy Todo with Haunted and Lit</h2>
            <br />
            <br />
            <button @click={fun _ -> dispatch LoadTodos}>Load Todos</button>
            <br />
            <br />

            {items}
            <br />
            <input 
                .value={state.AddNewValue} 
                @keyup={keyup}>
            <button @click={addTodo}>Add Todo</button>
            <br />
            <br />
        """



    let fancy_todo_lit_element (props: {| todos: string option |}) =
        let todoStr = defaultArg props.todos ""
        let todos = todoStr.Split([|","|], StringSplitOptions.RemoveEmptyEntries) |> Array.toList;

        let styling = fun () -> 
            css $""" 
                :host {{
                    background:grey;
                    overflow: auto;
                    font-family: sans-serif;
                }}

                li {{
                    display        : flex;
                    flex-direction : row;
                    justify-content: flex-start;
                    align-items    : center;
                    gap: 30px;
                }}

                button {{
                    padding      : 10px 46px;
                    background   : #303AB2 0%% 0%% no-repeat padding-box;
                    border-radius: 6px;
                    border       : none;
                    outline      : none;
                    cursor       : pointer;

                    font-size: 16px;
                    color    : #FFFFFF;
                }}

                button:hover {{
                    background-color: #172199;
                }}

                button:disabled {{
                    background-color: #7E84D1;
                }}

                button:active {{
                    background-color: #101980;
                }}

            """

        let state,dispatch = Haunted.useElmishWithStyling(init, update, todos, styling)

        viewLit state dispatch



    defineComponent "fancy-todo-lit" 
        (Haunted.Component(fancy_todo_lit_element, 
            {| 
                observedAttributes  = [| "todos" |]
                useShadowDom = true
            |}))
        

    
