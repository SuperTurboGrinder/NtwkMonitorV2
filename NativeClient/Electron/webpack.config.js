const webpack = require("webpack");
const path = require('path');

module.exports = {
    mode: 'production',
    target: 'electron-renderer',
    entry: {
        main: path.join(__dirname, 'src/main.ts'),
        renderer: path.join(__dirname, 'src/renderer.ts')
    },
    output: {
        path: path.join(__dirname, 'app/js'),
        filename: '[name].js'
    },
    optimization: {
        splitChunks: {
            name: 'vendor',
            chunks: "initial"
        }
    },
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: 'ts-loader',
                exclude: /node_modules/
            }
        ]
    },
    node: {
        fs: 'empty',
        __dirname: false,
        __filename: false
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js']
    },
};