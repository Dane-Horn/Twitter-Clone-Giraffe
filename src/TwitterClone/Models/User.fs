module TwitterClone.Models.User

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

open System.Security.Claims
open TwitterClone.Auth
open TwitterClone.DBAccess
open TwitterClone.DataModels
open System



let handleRegisterUser (next: HttpFunc) (ctx: HttpContext) =
    task {
        let row = Users.Create ()
        let! newUser = ctx.BindJsonAsync<User> ()
        let newId = (Guid.NewGuid ()).ToString ()
        row.Id <- newId
        row.Email <- newUser.Email
        row.Password <- newUser.Password
        row.Username <- newUser.Username
        try 
            dbctx.SubmitUpdates ()    
        with (e) ->
            dbctx.ClearUpdates () |> ignore
        let result = {Id = newId; Email = newUser.Email; Username = newUser.Username}
        let result = Map.empty.Add ("user", result)
        return! json result next ctx
    }
let handleMe = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        let id = ctx.User.FindFirst ClaimTypes.NameIdentifier
        let userMaybe= 
            query {
                for user in Users do
                where (user.Id = id.Value)
                select (Some {Id = user.Id; Email=user.Email; Username=user.Username})
                exactlyOneOrDefault
            }
        match userMaybe with
        | Some user ->
            let user = Map.empty.Add("user", user)
            json user next ctx
        | None ->
            text "User does not exist" next ctx
let handleLogin = 
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! model = ctx.BindJsonAsync<LoginModel> ()
            let email = model.Email
            let password = model.Password
            let idMaybe = query {
                for user in Users do
                    where (user.Email = email && user.Password = password)
                    select (Some (user.Id))
                    exactlyOneOrDefault
                }
            match idMaybe with
            | Some id -> 
                let token = Map.empty.Add("token", generateToken id)
                return! json token next ctx
            | None ->
                return! text "User does not exist" next ctx
        }