using System.Text;

// The server's counterpart to sw.js. Engaged only when the request carries
// "Content-Encoding: add-key": decodes the request before the handler sees it
// and encodes the handler's result. Encoding adds Key, decoding adds -Key.
class MathFilter(IMath math) : IEndpointFilter
{
    const string Name = "add-key";
    const int Key = 100;                              // must match sw.js
    readonly string[] supportedTypes = ["text/plain"];  // media types add-key can encode

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        var response = context.HttpContext.Response;

        // Whether the response is encoded depends on the request's Content-Encoding.
        response.Headers.Vary = "Content-Encoding";
        // Tell clients which content codings we accept in requests (RFC 9110 §12.5.3).
        response.Headers.AcceptEncoding = Name;

        var contentEncoding = request.Headers.ContentEncoding.ToString();
        if (contentEncoding is "" or "identity") return await next(context);   // bypass
        if (contentEncoding != Name || !IsSupported(request.ContentType))
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);

        // Request: decode the body and drop the header, so the handler sees plain content.
        var received = int.Parse(await new StreamReader(request.Body).ReadToEndAsync());
        var decoded = math.Add(received, -Key);
        request.Body = new MemoryStream(Encoding.UTF8.GetBytes(decoded.ToString()));
        request.Headers.Remove("Content-Encoding");

        // Response: only a string result is written as text/plain; anything else goes out unencoded.
        var output = await next(context);
        if (output is not string text) return output;

        var result = int.Parse(text);
        var encoded = math.Add(result, Key);
        response.Headers.ContentEncoding = Name;
        Console.WriteLine($"Received {received}, handler got {decoded}, handler returned {result}, sending {encoded}");
        return encoded.ToString();
    }

    // "text/plain; charset=utf-8" -> "text/plain"
    bool IsSupported(string? contentType) =>
        supportedTypes.Contains((contentType ?? "").Split(';')[0].Trim(), StringComparer.OrdinalIgnoreCase);
}
