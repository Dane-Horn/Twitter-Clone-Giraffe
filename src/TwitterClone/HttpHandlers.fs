namespace TwitterClone

module HttpHandlers =
    open Microsoft.AspNetCore.Http
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open TwitterClone.DataModels

    let handleGetHello =
        fun (next : HttpFunc) (ctx : HttpContext) ->
            task {
                let response = {
                    Id = "1"
                    Text = "Hello world, from Giraffe!"
                }
                return! json response next ctx
            }
    

