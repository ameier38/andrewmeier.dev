/** @type {import('tailwindcss').Config} */
module.exports = {
  content: ["./src/**/*.fs"],
  theme: {
    extend: {},
  },
  plugins: [
    require("@tailwindcss/typography"),
  ],
}
