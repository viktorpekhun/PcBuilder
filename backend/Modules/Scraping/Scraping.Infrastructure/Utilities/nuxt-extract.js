
const fs = require('fs');

let input = fs.readFileSync(0, 'utf8');
try {
    const window = {};
    eval(input);
    const result = JSON.stringify(window.__NUXT__);
    console.log(result);
} catch (e) {
    console.error("JS Error:", e.message);
    process.exit(1);
}