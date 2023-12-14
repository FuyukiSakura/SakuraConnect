/** @type {import('tailwindcss').Config} */
module.exports = { 
    prefix: "tw-",
    content: ["./**/*.{razor,html,cshtml}"],
    theme: {
        extend: {
            fontFamily: {
                sans: ['Inter', 'sans-serif']
            }
        }
    },
    plugins: []
}
