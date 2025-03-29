// This file helps with TypeScript module resolution

// Allow JSX in .tsx files
declare namespace JSX {
  interface IntrinsicElements {
    [elemName: string]: any;
  }
}

// Fix for modules that use 'export =' syntax
declare module 'react' {
  import React = require('react');
  export = React;
}

declare module 'prop-types' {
  import PropTypes = require('prop-types');
  export = PropTypes;
}

// Path aliases
declare module '@/hooks/*' {
  const content: any;
  export default content;
}

declare module '@/services/*' {
  const content: any;
  export default content;
}

declare module '@/utils/*' {
  const content: any;
  export default content;
}
