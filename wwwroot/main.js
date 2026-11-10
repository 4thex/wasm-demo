// Register the service worker and wait until it controls this page.
async function ready() {
    await navigator.serviceWorker.register('sw.js', { type: 'module' });
    if (navigator.serviceWorker.controller) return;
    await new Promise(r => navigator.serviceWorker.addEventListener('controllerchange', r, { once: true }));
}

await ready();

// When on, the request is marked "Content-Encoding: add-key". The service worker
// then encodes it, and the server answers with an add-key encoded response.
let encoding = true;
const toggle = document.getElementById('toggle');
const render = () => toggle.textContent = `Content-Encoding: ${encoding ? 'add-key' : '(none)'}`;
toggle.addEventListener('click', () => { encoding = !encoding; render(); });
render();

document.getElementById('send').addEventListener('click', async () => {
    const response = await fetch('/api/add-two', {
        method: 'POST',
        headers: encoding ? { 'Content-Encoding': 'add-key' } : {},
        body: document.getElementById('input').value,
    });
    document.getElementById('output').textContent =
        `${response.status} ${response.statusText}\n` +
        [...response.headers].map(([k, v]) => `${k}: ${v}`).join('\n') +
        `\n\n${await response.text()}`;
});
