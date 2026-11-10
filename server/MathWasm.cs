using Wasmtime;

// IMath backed by math.wasm, the same file the browser downloads.
// Loaded once; registered as a singleton.
class MathWasm : IMath
{
    readonly Func<int, int, int> add;
    readonly object gate = new();   // a wasm Store is not thread-safe

    public MathWasm(IWebHostEnvironment env)
    {
        var engine = new Engine();
        var module = Module.FromFile(engine, Path.Combine(env.WebRootPath, "math.wasm"));
        var instance = new Linker(engine).Instantiate(new Store(engine), module);
        add = instance.GetFunction<int, int, int>("add")!;
    }

    public int Add(int a, int b)
    {
        lock (gate) return add(a, b);
    }
}
