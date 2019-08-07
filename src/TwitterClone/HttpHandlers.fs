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
    let handleGetComp (next : HttpFunc) (ctx : HttpContext) =
        let rec fibo n =
            if n < 0 then
                0
            else if n = 1 then
                1
            else
                (fibo (n - 1)) + (fibo (n - 2))
        json (Map.empty.Add ("message", fibo 30)) next ctx
    

