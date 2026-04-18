/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './index.html',
    './src/**/*.{js,jsx}',
    './node_modules/@aegov/design-system-react/**/*.{js,jsx}',
  ],
  theme: {
    extend: {
      colors: {
        'tenant-primary':   'var(--tenant-primary)',
        'tenant-secondary': 'var(--tenant-secondary)',
      },
      fontFamily: {
        arabic: ['Cairo', 'Tajawal', 'sans-serif'],
      },
      keyframes: {
        'dialog-in': {
          '0%':   { opacity: '0', transform: 'scale(0.95) translateY(-8px)' },
          '100%': { opacity: '1', transform: 'scale(1)   translateY(0)'     },
        },
      },
      animation: {
        'dialog-in': 'dialog-in 0.15s ease-out',
      },
    },
  },
  plugins: [],
}
