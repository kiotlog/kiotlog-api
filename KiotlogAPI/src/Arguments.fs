(*
    Copyright (C) 2017 Giampaolo Mancini, Trampoline SRL.
    Copyright (C) 2017 Francesco Varano, Trampoline SRL.

    This file is part of Kiotlog.

    Kiotlog is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Kiotlog is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace Kiotlog.Web

open System
open System.Net
open Argu

module Arguments =
    type Configuration =
        {
            HttpHost: string
            HttpPort: int
            PostgresUser: string
            PostgresPass: string
            PostgresHost: string
            PostgresPort: int
            PostgresDb: string
            ApiKey: string
        }

    let private getPostgresConnectionString c =
        sprintf
            "Username=%s;Password=%s;Host=%s;Port=%d;Database=%s"
            c.PostgresUser c.PostgresPass c.PostgresHost c.PostgresPort c.PostgresDb

    type Configuration with
        member c.PostgresConnectionString = getPostgresConnectionString c

    type KiotlogAPIArgs =
        | Host of webhost:string
        | Port of webport:int
        | PgUser of pguser:string
        | PgPass of pgpass:string
        | PgHost of pghost:string
        | PgPort of pgport:int
        | PgDb of db:string
        | ApiKey of apikey:string
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Host _ -> "HTTP address or name"
                | Port _ -> "HTTP port"
                | PgUser _ -> "Postgres username"
                | PgPass _ -> "Postgres password"
                | PgHost _ -> "Postgres host"
                | PgPort _ -> "Postgres port db"
                | PgDb _ -> "Postgres db"
                | ApiKey _ -> "Api Key"

    let private errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.Red)
    let private parser = ArgumentParser.Create<KiotlogAPIArgs>(programName = "KiotlogAPI", errorHandler = errorHandler)

    let parseENV =
        let reader = ConfigurationReader.FromEnvironmentVariables()
        let results = parser.ParseConfiguration reader
        {
            HttpHost = results.GetResult(<@ Host @>, defaultValue = string IPAddress.Loopback)
            HttpPort = results.GetResult(<@ Port @>, defaultValue = 8888)
            PostgresUser = results.GetResult(<@ PgUser @>, defaultValue = "postgres")
            PostgresPass = results.GetResult(<@ PgPass @>, defaultValue = "postgres")
            PostgresHost = results.GetResult(<@ PgHost @>, defaultValue = "localhost")
            PostgresPort = results.GetResult(<@ PgPort @>, defaultValue = 5432)
            PostgresDb = results.GetResult(<@ PgDb @>, defaultValue = "postgres")
            ApiKey = results.GetResult(<@ ApiKey @>, defaultValue = "")
        }

    let parseCLI argv =
        let env = parseENV
        let results = parser.ParseCommandLine argv
        {
            HttpHost = results.GetResult(<@ Host @>, defaultValue = env.HttpHost)
            HttpPort = results.GetResult(<@ Port @>, defaultValue = env.HttpPort)
            PostgresUser = results.GetResult(<@ PgUser @>, defaultValue = env.PostgresUser)
            PostgresPass = results.GetResult(<@ PgPass @>, defaultValue = env.PostgresPass)
            PostgresHost = results.GetResult(<@ PgHost @>, defaultValue = env.PostgresHost)
            PostgresPort = results.GetResult(<@ PgPort @>, defaultValue = env.PostgresPort)
            PostgresDb = results.GetResult(<@ PgDb @>, defaultValue = env.PostgresDb)
            ApiKey = results.GetResult(<@ ApiKey @>, defaultValue = "")
        }

//    let config = parser.ParseConfiguration
