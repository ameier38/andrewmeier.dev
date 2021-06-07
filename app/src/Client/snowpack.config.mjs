// Snowpack Configuration File
// See all supported options: https://www.snowpack.dev/reference/configuration

import proxy from 'http2-proxy';

/** @type {import("snowpack").SnowpackUserConfig } */
export default {
  mount: {
    static: '/',
    scripts: '/scripts',
    styles: '/styles',
    compiled: '/compiled'
  },
  plugins: [
    '@snowpack/plugin-react-refresh',
    '@snowpack/plugin-postcss'
  ],
  packageOptions: {},
  devOptions: {
    port: 3000,
    output: 'stream',
    open: 'none',
    tailwindConfig: './tailwind.config.mjs',
  },
  buildOptions: {
    out: 'out'
  },
  routes: [
    { src: '/api/.*', dest: (req, res) => proxy.web(req, res, { hostname: 'localhost', port: 5000 }) },
    { match: "routes", src: ".*", dest: "/index.html" }
  ]
};
