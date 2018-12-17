const webpack = require("webpack");
const path = require('path');
const fs = require('fs');

const nodeModules = {};
fs.readdirSync('node_modules')
  .filter(function(x) {
    return ['.bin'].indexOf(x) === -1;
  })
  .forEach(function(mod) {
    nodeModules[mod] = 'commonjs ' + mod;
  });

module.exports = {
    mode: 'production',
    target: 'node',
    entry: {
        electron_packager_build: path.join(__dirname, 'electron-packager-config/electron-packager.build.ts'),
        electron_packager_rebuild: path.join(__dirname, 'electron-packager-config/electron-packager.rebuild.ts')
    },
    output: {
        path: path.join(__dirname, 'electron-packager-config/js'),
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
        __dirname: false,
    },
    resolve: {
        extensions: ['.ts', '.tsx', '.js']
    },
    externals: nodeModules
};