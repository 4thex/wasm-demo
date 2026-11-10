// Plain server code: knows nothing about math.wasm or the encoding.
class AddTwoHandler
{
    // Returns a string so the response is text/plain (an int would be serialized as JSON).
    public async Task<string> Handle(HttpRequest request)
    {
        var n = int.Parse(await new StreamReader(request.Body).ReadToEndAsync());
        return (n + 2).ToString();
    }
}
