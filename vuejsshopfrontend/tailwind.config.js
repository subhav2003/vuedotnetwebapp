/** @type {import('tailwindcss').Config} */
  export default {
    content: [
      './index.html',
      './src/**/*.{vue,js,ts,jsx,tsx}', // Add this to scan all your Vue files for Tailwind classes
    ],
    theme: {
      extend: {fontFamily: {
          papyrus: ['Papyrus', 'serif'],
        },
        colors: {
          'brown-500': '#A0522D', // Customize the brown color as needed
          'brown-600': '#8B4513',
        },
      },
    },
    plugins: [],
  }
