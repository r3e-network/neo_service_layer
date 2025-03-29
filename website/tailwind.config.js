/** @type {import('tailwindcss').Config} */
module.exports = {
  // Enable JIT mode for faster builds
  mode: 'jit',
  // Optimize content scanning
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
    './src/utils/**/*.{js,ts,jsx,tsx}',
  ],
  // Optimize by disabling unused features in production
  darkMode: 'class',
  future: {
    hoverOnlyWhenSupported: true,
    respectDefaultRingColorOpacity: true,
    disableColorOpacityUtilitiesByDefault: true,
    purgeLayersByDefault: true,
  },
  theme: {
    extend: {
      colors: {
        'neo-green': '#00E599',
        'neo-dark': '#1E2B34',
        'neo-light': '#F0F3F5',
        neo: {
          green: '#00E599',
          dark: '#121212',
        },
      },
      typography: {
        DEFAULT: {
          css: {
            maxWidth: '65ch',
            color: 'inherit',
            a: {
              color: '#00E599',
              textDecoration: 'none',
              '&:hover': {
                color: '#00B377',
              },
            },
            strong: {
              color: 'inherit',
            },
            code: {
              color: 'inherit',
              background: 'rgba(0, 229, 153, 0.1)',
              borderRadius: '0.25rem',
              padding: '0.25rem 0.5rem',
            },
            pre: {
              background: '#1E2B34',
              color: '#F0F3F5',
              borderRadius: '0.25rem',
              padding: '1rem',
            },
          },
        },
      },
      fontFamily: {
        sans: ['Inter var', 'sans-serif'],
      },
      animation: {
        'slide-up': 'slideUp 0.5s ease-in-out',
        'fade-in': 'fade-in 0.8s ease-out forwards',
        'fade-in-up': 'fade-in-up 0.8s ease-out forwards',
        'fade-in-scale': 'fade-in-scale 0.8s ease-out forwards',
      },
      keyframes: {
        slideUp: {
          '0%': { transform: 'translateY(20px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        'fade-in': {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        'fade-in-up': {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        'fade-in-scale': {
          '0%': { opacity: '0', transform: 'scale(0.95)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-conic':
          'conic-gradient(from 180deg at 50% 50%, var(--tw-gradient-stops))',
      },
      transitionDelay: {
        '200': '200ms',
        '400': '400ms',
        '600': '600ms',
        '800': '800ms',
      },
    },
  },
  // Optimize variants for production
  variants: {
    extend: {
      opacity: ['disabled'],
      cursor: ['disabled'],
    },
  },
  // Add plugins
  plugins: [
    require('@tailwindcss/typography'),
  ],
  // Production optimizations
  corePlugins: {
    // Disable features not used in the project
    container: false,
    placeholderOpacity: false,
    divideOpacity: false,
    backgroundOpacity: false,
    borderOpacity: false,
  },
};