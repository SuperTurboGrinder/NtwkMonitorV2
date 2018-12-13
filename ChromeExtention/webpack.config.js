const webpack = require("webpack");
const path = require('path');

module.exports = {
    mode: 'production',
    entry: {
        plugin: path.join(__dirname, 'src/plugin.ts')
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
    resolve: {
        extensions: ['.ts', '.tsx', '.js']
    },
};