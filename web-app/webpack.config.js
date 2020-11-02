// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

const DotenvPlugin = require("dotenv-webpack")
var webpack = require("webpack")
var path = require("path");

module.exports = (env, argv) => {
    const mode = argv.mode
    console.log(mode)

    return {
        mode: mode,
        entry: "./src/App.fsproj",
        output: {
            path: path.join(__dirname, "dist"),
            filename: "main.js"
        },
        devServer: {
            contentBase: path.join(__dirname, "dist"),
            hot: true,
            inline: true,
            historyApiFallback: true,
            port: 3000
        },
        plugins: mode === "development" ?
            // development plugins
            [
                new DotenvPlugin({
                    path: path.join(__dirname, '.env'),
                    silent: true,
                    systemvars: true
                }),
                new webpack.HotModuleReplacementPlugin(),
            ]
            :
            // production plugins
            [
                new DotenvPlugin({
                    path: path.join(__dirname, '.env'),
                    silent: true,
                    systemvars: true
                }),
            ],
        module: {
            rules: [
                { 
                    test: /\.fs(x|proj)?$/, 
                    use: { 
                        loader:  "fable-loader", 
                        options: { 
                            define: mode === "development" ? ["DEVELOPMENT"]: [] 
                        } 
                    }
                },
                { 
                    test: /\.css$/, 
                    use: ['style-loader', 'css-loader']
                },
            ]
        }
    }
}
