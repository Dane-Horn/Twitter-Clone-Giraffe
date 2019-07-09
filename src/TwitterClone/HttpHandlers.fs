namespace TwitterClone
open System.Security.Claims

module HttpHandlers =

    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open TwitterClone.Models.Tweet
    open TwitterClone.DBAccess
    open TwitterClone.Auth
    open System

    type LoginModel = {
        Email: string
        Password: string
    }

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response = {
                    Id = "1"
                    Text = "Hello world, from Giraffe!"
                }
                return! json response next ctx
            }
    let handleGetSecure = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            let email = ctx.User.FindFirst ClaimTypes.NameIdentifier
            text (email.Value) next ctx
    let handlePostSecure = 
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! model = ctx.BindJsonAsync<LoginModel> ()
                let token = generateToken model.Email
                return! text token next ctx
            }

