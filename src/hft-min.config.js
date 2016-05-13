const webpack = require('webpack');
const path = require('path');
var plugins = require('webpack-load-plugins')();

module.exports = {
    entry: './hft.js',
    output: {
        path: path.join(__dirname, '../Assets/WebPlayerTemplates/HappyFunTimes/hft'),
        filename: 'hft-min.js',
    },
    plugins: [
        new webpack.optimize.UglifyJsPlugin({
            compress: {
                side_effects: false,
            },
            output: {
                comments: false,
            },
        }),
    ],
}

