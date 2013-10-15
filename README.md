# FShake

Provides abstractions for building, loosely inspired by Neil Mitchell's
Haskell Shake library.

* Like `make`, allows to express dependencies between targets and do minimal
  rebuilds. Unlike `make`, targets are fully abstract, dependencies can be
  dynamic, and you get the power of F#.

* Entry points are `Builder.BuildTarget`, `Builder.CleanTarget`.

* `Rules` is a rule set defining how things are built. It is a partial
  map from targets to recipes defining how to build them. Forms a monoid.

* The elementary `Rules` value defines a partial map from `'Key` to `'Value`,
  given some appropriate `ITargetable<'Key,'Value>` constraints - serialization,
  equality, and so on.

* `Recipe<'T>` is a monad defining build actions. Like `Async`, but you
  can also require targets which are build according to an implicit `Rules`.
  This gives recursive dependency resolution and dynamic dependencies.

* When a `Recipe<'T>` is built, the end result together with a trace of
  all required targets (`Snapshot<'T>`) is serialized to the disk-based
  database. Subsequent builds of the same target lookup the snapshot from disk,
  and if it is still valid, return it instead of building the `Recipe`.

