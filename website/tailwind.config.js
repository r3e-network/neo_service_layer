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
        primary: '#00E599',
        secondary: '#00AEFF',
        accent: '#7928CA',
        'neo-green': '#00E599',
        'neo-dark': '#1E2B34',
        'neo-light': '#F0F3F5',
        neo: {
          green: '#00E599',
          dark: '#121212',
          blue: '#00AEFF',
          purple: '#7928CA',
        },
        gray: {
          50: '#F7F9FC',
          100: '#EDF1F7',
          200: '#E4E9F2',
          300: '#C5CEE0',
          400: '#8F9BB3',
          500: '#2E3A59',
          600: '#222B45',
          700: '#1A2138',
          800: '#151A30',
          900: '#101426',
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
        'fade-in': 'fadeIn 0.8s ease-out forwards',
        'fade-in-up': 'fadeInUp 0.8s ease-out forwards',
        'fade-in-scale': 'fadeInScale 0.8s ease-out forwards',
        'float': 'float 3s ease-in-out infinite',
        'pulse': 'pulse 2s ease-in-out infinite',
        'shimmer': 'shimmer 2s infinite linear',
      },
      keyframes: {
        slideUp: {
          '0%': { transform: 'translateY(20px)', opacity: '0' },
          '100%': { transform: 'translateY(0)', opacity: '1' },
        },
        fadeIn: {
          '0%': { opacity: '0' },
          '100%': { opacity: '1' },
        },
        fadeInUp: {
          '0%': { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeInScale: {
          '0%': { opacity: '0', transform: 'scale(0.95)' },
          '100%': { opacity: '1', transform: 'scale(1)' },
        },
        float: {
          '0%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-10px)' },
          '100%': { transform: 'translateY(0px)' },
        },
        pulse: {
          '0%': { transform: 'scale(1)', opacity: '1' },
          '50%': { transform: 'scale(1.05)', opacity: '0.8' },
          '100%': { transform: 'scale(1)', opacity: '1' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-1000px 0' },
          '100%': { backgroundPosition: '1000px 0' },
        },
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-conic':
          'conic-gradient(from 180deg at 50% 50%, var(--tw-gradient-stops))',
        'neo-gradient': 'linear-gradient(135deg, #00E599 0%, #00D1FF 100%)',
      },
      transitionDelay: {
        '200': '200ms',
        '400': '400ms',
        '600': '600ms',
        '800': '800ms',
      },
      boxShadow: {
        'neo': '0 4px 14px rgba(0, 229, 153, 0.5)',
        'card': '0px 8px 24px rgba(0, 0, 0, 0.06)',
        'card-hover': '0px 12px 30px rgba(0, 0, 0, 0.08)',
      },
      borderRadius: {
        'xl': '1rem',
        '2xl': '1.5rem',
        '3xl': '2rem',
      },
    },
  },
  // Optimize variants for production
  variants: {
    extend: {
      opacity: ['disabled'],
      cursor: ['disabled'],
      scale: ['group-hover'],
      transform: ['group-hover'],
    },
  },
  // Add plugins
  plugins: [
    require('@tailwindcss/typography'),
  ],
  // Disable unused core plugins for better performance
  corePlugins: {
    container: false,
    placeholderOpacity: false,
    divideOpacity: false,
    backgroundOpacity: false,
    borderOpacity: false,
  },
};