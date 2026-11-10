// Service worker: when a request to /api/ carries "Content-Encoding: add-key",
// encode its body and decode an add-key response using math.add().
// Everything else passes straight through. The counterpart to server/MathFilter.cs.

import { math } from './math-wasm.js';

const ENCODING = 'add-key';
const KEY = 100;                        // must match MathFilter.cs
const SUPPORTED_TYPES = ['text/plain'];  // media types add-key can encode

// "text/plain;charset=UTF-8" -> "text/plain"
const mediaType = headers => (headers.get('Content-Type') ?? '').split(';')[0].trim().toLowerCase();

self.addEventListener('install', () => self.skipWaiting());
self.addEventListener('activate', e => e.waitUntil(self.clients.claim()));

self.addEventListener('fetch', event => {
    const { request } = event;
    if (!new URL(request.url).pathname.startsWith('/api/')) return;
    if (request.headers.get('Content-Encoding') !== ENCODING) return;   // bypass
    event.respondWith(handle(request));
});

async function handle(request) {
    // Keep the page's headers (including Content-Type); only Content-Encoding is ours.
    const headers = new Headers(request.headers);

    // A type add-key can't encode: send it plain, and don't claim it is encoded.
    if (!SUPPORTED_TYPES.includes(mediaType(request.headers))) {
        headers.delete('Content-Encoding');
        return fetch(request.url, { method: request.method, headers, body: await request.arrayBuffer() });
    }

    // Request: encode the body.
    const encoded = await math.add(Number(await request.text()), KEY);
    const response = await fetch(request.url, { method: request.method, headers, body: String(encoded) });

    // Response: decode only if the server actually encoded it.
    if (response.headers.get('Content-Encoding') !== ENCODING) return response;
    const decoded = await math.add(Number(await response.text()), -KEY);
    const responseHeaders = new Headers(response.headers);
    responseHeaders.delete('Content-Encoding');
    responseHeaders.delete('Content-Length');
    return new Response(String(decoded), { status: response.status, statusText: response.statusText, headers: responseHeaders });
}
