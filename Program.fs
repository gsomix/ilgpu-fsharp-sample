// Learn more about F# at http://fsharp.org

open ILGPU
open ILGPU.Runtime

type Program =
    static member MyKernel (index: Index1) (dataView: ArrayView<int>) (constant: int) =
        dataView.[index] <- int(index + Index1(constant))

[<EntryPoint>]
let main argv =
    printfn "Hello World from F#!"

    use context = new Context()
    for acceleratorId in Accelerator.Accelerators do
        use accelerator = Accelerator.Create(context, acceleratorId)
        printfn "Performing operations on %O" accelerator
        
        let kernel = accelerator.LoadAutoGroupedStreamKernel<_, _, _>(Program.MyKernel)
        use buffer = accelerator.Allocate<_>(1024L)
        
        kernel.Invoke(Index1(int(buffer.Length)), buffer.View, 42)
        accelerator.Synchronize()
        
        buffer.GetAsArray()
        |> Array.iteri (fun index element ->
            if element <> 42 + index
            then printfn "Error at element location %d: %d found" index element)

    0 // return an integer exit code
