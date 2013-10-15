namespace FShake

open System

type Targetable<'Key,'Value> =
    {
        Instance : ITargetable<'Key,'Value>
        Type : Type
    }

module Targetable =

    let Create (s: Singleton<#ITargetable<'K,'V>>) =
        {
            Instance = s.Instance
            Type = s.Type
        }

