namespace Server

open Newtonsoft.Json
open Newtonsoft.Json.Linq
open FSharp.Data.GraphQL
open FSharp.Data.GraphQL.Types
open FSharp.Data.GraphQL.Types.Patterns
open System

type GraphQLQuery =
    { ExecutionPlan : ExecutionPlan
      Variables : Map<string, obj> }

[<Sealed>]
type GraphQLQueryConverter<'a>(executor : Executor<'a>) =
    inherit JsonConverter()

    override __.CanConvert(t) = t = typeof<GraphQLQuery>  
    
    override __.WriteJson(_, _, _) =  failwith "Not supported"    

    override __.ReadJson(reader, _, _, serializer) =
        let jobj = JObject.Load reader
        let query = 
            if jobj.ContainsKey("query") then
                match jobj.Property("query").Value.ToString().Replace("\r\n", " ").Replace("\n", " ") with
                | "{}" -> Introspection.IntrospectionQuery
                | s when String.IsNullOrEmpty(s) -> Introspection.IntrospectionQuery
                | qry -> qry
            else jobj.ToString() |> failwithf "invalid request; missing 'query'\n%s"
        let operationNameOpt = 
            if jobj.ContainsKey("operationName") then
                let operationName = jobj.Property("operationName").Value.ToString()
                printfn "operationName: %s" operationName
                Some operationName
            else None
        let plan = 
            match operationNameOpt with
            | Some opName -> executor.CreateExecutionPlan(query, operationName = opName)
            | None -> executor.CreateExecutionPlan(query)
        let varDefs = plan.Variables
        match varDefs with
        | [] -> upcast { ExecutionPlan = plan; Variables = Map.empty }
        | vs ->
            let vars = JObject.Parse(jobj.Property("variables").Value.ToString())
            let variables = 
                vs
                |> List.fold (fun (acc: Map<string, obj>)(vdef: VarDef) ->
                    match vars.TryGetValue(vdef.Name) with
                    | true, jval ->
                        let v = 
                            match jval.Type with
                            | JTokenType.Null -> null
                            | JTokenType.String -> jval.ToString() :> obj
                            | _ -> jval.ToObject(vdef.TypeDef.Type, serializer)
                        Map.add (vdef.Name) v acc
                    | false, _  ->
                        match vdef.DefaultValue, vdef.TypeDef with
                        | Some _, _ -> acc
                        | _, Nullable _ -> acc
                        | None, _ -> failwithf "Variable %s has no default value and is missing!" vdef.Name) Map.empty
            upcast { ExecutionPlan = plan; Variables = variables }
