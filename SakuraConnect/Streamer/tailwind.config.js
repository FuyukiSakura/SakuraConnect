/** @type {import('tailwindcss').Config} */
module.exports = {
  prefix: 'tw-',
  content: ["./**/*.{razor,cshtml,html}"],
  theme: {
    extend: {},
  },
  plugins: [],
  corePlugins: {
      preflight: false,
  },
}
