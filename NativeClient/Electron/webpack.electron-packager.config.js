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
const locales = ['en', 'ru-ru'];
const builds = {};
const baseDir = path.join(__dirname, 'electron-packager-config');
for (locale of locales) {
    builds[`build_${locale.replace('-', '_')}`] =
        path.join(baseDir, `build.${locale}.ts`);
    builds[`rebuild_${locale.replace('-', '_')}`] =
        path.join(baseDir, `rebuild.${locale}.ts`);
}

module.exports = {
    mode: 'production',
    target: 'node',
    entry: builds,
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