module TwitterClone.Models.Auth

open Microsoft.AspNetCore.Http
open System.Security.Claims
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open TwitterClone.Auth
open TwitterClone.DBAccess

type LoginModel = {
        Email: string
        Password: string
    }

let handleGetSecure = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let id = ctx.User.FindFirst ClaimTypes.NameIdentifier
        let userMaybe = query {
            for user in Users do
            where (user.Id = id.Value)
            select (Some user)
            exactlyOneOrDefault
        }
        match userMaybe with
        | Some user ->
            let user = Map.empty.Add("user", user.ColumnValues |> Map.ofSeq)
            json user next ctx
        | None ->
            text "User does not exist" next ctx
let handlePostSecure = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindJsonAsync<LoginModel> ()
            let email = model.Email
            let userMaybe = query {
                for user in Users do
                    where (user.Email = email)
                    select (Some user)
                    exactlyOneOrDefault
                }
            match userMaybe with
            | Some user -> 
                let token = Map.empty.Add("token", generateToken user.Id)
                return! json token next ctx
            | None ->
                return! text "User does not exist" next ctx
        }
