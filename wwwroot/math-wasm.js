// Loads math.wasm once and exposes add(). The JS counterpart to server/MathWasm.cs.

const exports = fetch('math.wasm')
    .then(r => r.arrayBuffer())
    .then(bytes => WebAssembly.instantiate(bytes, {}))
    .then(({ instance }) => instance.exports);

export const math = {
    async add(a, b) {
        return (await exports).add(a, b);
    },
};
