module TwitterClone.Models.User

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

open TwitterClone.DBAccess
open System

[<CLIMutable>]
type User = {
    Id: string
    Email: string
    Password: string
}

let handlePostUser (email: string, password: string) (next: HttpFunc) (ctx: HttpContext) =
    let row = Users.``Create(email, password)`` (email, password)
    let newId = (Guid.NewGuid ()).ToString ()
    row.Id <- newId
    dbctx.SubmitUpdates ()
    json row next ctx