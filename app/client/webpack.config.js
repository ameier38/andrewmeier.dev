const DotenvPlugin = require('dotenv-webpack')
const HtmlPlugin = require('html-webpack-plugin')
const path = require('path')
const webpack = require('webpack');

module.exports = (env, argv) => {
    const mode = argv.mode

    const htmlPlugin = new HtmlPlugin({
        template: path.join(__dirname, 'static', 'index.html'),
        favicon: path.join(__dirname, 'static', 'favicon.svg')
    })

    const dotenvPlugin = new DotenvPlugin({
        silent: true,
        systemvars: true
    })

    return {
        mode: mode,
        entry: './bin/Program.js',
        output: {
            path: path.join(__dirname, "dist"),
            filename: "main.js",
        },
        devServer: {
            port: 3000,
            hot: true,
            inline: true,
            // NB: required so that webpack will go to index.html on not found
            historyApiFallback: true,
            proxy: {
                '/api/*': {
                    target: 'http://localhost:5000',
                    changeOrigin: true
                }
            }
        },
        // NB: so webpack works with docker
        watchOptions: {
            poll: true
        },
        plugins: mode === 'development' ?
            [
                dotenvPlugin,
                htmlPlugin,
                new webpack.HotModuleReplacementPlugin(),
            ]
            :
            [
                dotenvPlugin,
                htmlPlugin,
            ],
        module: {
            rules: [
                { 
                    test: /\.js$/,
                    exclude: /node_modules/,
                    use: {
                        loader: 'babel-loader',
                        options: {
                            presets: ['@babel/preset-env'],
                            plugins: [
                                ['prismjs', {
                                    "languages": [
                                        "javascript",
                                        "shell-session",
                                        "python",
                                        "fsharp"
                                    ],
                                    "plugins": [
                                        "line-numbers"
                                    ],
                                    "theme": "okaidia",
                                    "css": true
                                }]
                            ]
                        }
                    }
                },
                {
                    test: /\.(png|jpe?g|gif|svg)$/i,
                    use: "file-loader"
                },
                { 
                    test: /\.css$/, 
                    use: ['style-loader', 'css-loader']
                },
            ]
        }
    }
}
