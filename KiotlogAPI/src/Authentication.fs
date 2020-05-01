module Kiotlog.Web.Authentication

open Suave
open Suave.RequestErrors

// let apiKey = "" // "83a1ab6a-52a5-48f5-836d-79662722345b"

let internal parseAuthenticationToken (authorization : string) =
  let parts = authorization.Split (' ')
  match parts.Length with
  | 2 ->
      let token = parts.[1].Trim()
      match parts.[0].ToLowerInvariant() with
      | "bearer" -> Ok token
      | _ -> Error "No Bearer Token"
  | _ -> Error "Wrong format for Authorization Header (need: Authorization: Bearer <token>)"

let internal getToken (request: HttpRequest) =
    match request.header "authorization" with
    | Choice1Of2 header -> parseAuthenticationToken header
    | Choice2Of2 _ -> Error "No Authorization Header"

let internal checkToken (apiKey: string) (token: string) =
    if token = apiKey then
        Ok ""
    else    
        Error "Not Authorized"

let authenticate apiKey webpart (ctx: HttpContext) =
    async {
        let authResult =
            if String.isEmpty apiKey then Ok "" else getToken ctx.request |> Result.bind (checkToken apiKey)
        match authResult with
        | Ok _ -> return! webpart ctx
        | Error e -> return! UNAUTHORIZED e ctx
    }