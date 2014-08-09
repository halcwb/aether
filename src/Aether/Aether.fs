﻿namespace Aether


/// Partial lens ('a -> 'b option) * ('b -> 'a -> 'a)
type LensP<'a,'b> = ('a -> 'b option) * ('b -> 'a -> 'a)

/// Total lens ('a -> 'b) * ('b -> 'a -> 'a)
type LensT<'a,'b> = ('a -> 'b) * ('b -> 'a -> 'a)


[<AutoOpen>]
module Functions =

    let internal getL (g, _) = g
    let internal setL (_, s) = s

    /// Get a value option using a Partial lens
    let getP (l: LensP<'a,'b>) = getL l

    /// Set a value using a Partial lens
    let setP (l: LensP<'a,'b>) = setL l

    /// Update a value using a Partial lens
    let updP (l: LensP<'a,'b>) = fun f a -> Option.map f (getL l a) |> function | Some b -> setL l b a | _ -> a

    /// Get a value using a Total lens
    let getT (l: LensT<'a,'b>) = getL l

    /// Set a value using a Total lens
    let setT (l: LensT<'a,'b>) = setL l

    /// Update a value using a Total lens
    let updT (l: LensT<'a,'b>) = fun f a -> setL l (f (getL l a)) a


[<AutoOpen>]
module internal Composition =

    let tx l1 l2  =
        (fun a -> getL l2 (getL l1 a)),
        (fun c a -> setL l1 (setL l2 c (getL l1 a)) a)

    let pt (l1: LensP<'a,'b>) (l2: LensT<'b,'c>) = //: LensP<'a,'c> =
        (fun a -> Option.map (getL l2) (getL l1 a )),
        (fun c a -> a)

    let pp l1 l2 =
        (fun a -> Option.bind (getL l2) (getL l1 a)),
        (fun c a -> Option.map (setL l2 c) (getL l1 a) |> function | Some b -> setL l1 b a | _ -> a)


[<AutoOpen>]
module Operators =

    /// Compose two Total lenses, giving a Total lens
    let (>-->) (t1: LensT<'a,'b>) (t2: LensT<'b,'c>) : LensT<'a,'c> = tx t1 t2

    /// Compose a Total lens and a Partial lens, giving a Partial lens
    let (>-?>) (t1: LensT<'a,'b>) (p1: LensP<'b,'c>) : LensP<'a,'c> = tx t1 p1

    /// Compose a Total lens and a Partial lens, giving a Partial lens
    let (>?->) (p1: LensP<'a,'b>) (t1: LensT<'b,'c>) : LensP<'a,'c> = pt p1 t1

    /// Compose two Partial lenses, giving a Partial lens
    let (>??>) (p1: LensP<'a,'b>) (p2: LensP<'b,'c>) : LensP<'a,'c> = pp p1 p2